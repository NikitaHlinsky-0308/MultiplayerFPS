using UnityEngine;
using UnityEngine.Serialization;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{

    [SerializeField] private Transform viewPoint;
    public float mouseSensetivity = 1f;
    private float _verticalRotStore;
    private Vector2 _mouseInput;
    public bool invertLook = false;
    [SerializeField] private Camera cam;

    public float moveSpeed = 5f, runSpeed = 8f;
    private float _activeMoveSpeed;
    public Vector3 moveDir, movement;
    public float gravityMyltiplier = 1f;
    public float jumpForse;

    public bool isGrounded;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;

    //[SerializeField] private float timeBetweenShoots;
    private float _shootsCount;
    private float _muzzleCount;
    [SerializeField] private float muzzleDisplayTime;
    public float maxHeat = 10f, /* heatPerShoot = 1f,*/ coolRate = 4f, overHeatCoolRate = 5f;
    private float _heatCounter;
    private bool _overHeated;

    [SerializeField] private Weapon[] allWeapons;
    private int _selectedGun = 0;
    
    [FormerlySerializedAs("bulletImpact")] [SerializeField] private GameObject bulletImpactPrefab;

    public GameObject playerHitImpact;
    public CharacterController characterCon;

    public float maxHealth = 100f;
    public float currentHealth;

    public Animator anim;
    public GameObject playerModel; 
    public Transform modelGunPoint, gunHolder;

    public float adsSpeed = 5.0f;
    public Transform adsInPoint, adsOutPoint;

    public AudioSource footstepSlow, footstepFast; 


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIManager.instance.orevheatedSlider.value = maxHeat;
        
        //SwitchWeapon();
        photonView.RPC("SetWeapon", RpcTarget.All, _selectedGun);
        
        currentHealth = maxHealth;
        

        if (photonView.IsMine)
        {
            playerModel.SetActive(false);
            
            UIManager.instance.currentHealthSlider.maxValue = maxHealth;
            UIManager.instance.currentHealthSlider.value = currentHealth;
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }
    
    void Update()
    {
        if (photonView.IsMine)
        {


            _mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensetivity;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + _mouseInput.x, transform.rotation.eulerAngles.z);
            _verticalRotStore += _mouseInput.y;
            _verticalRotStore = Mathf.Clamp(_verticalRotStore, -60, 60);
            if (invertLook)
            {
                viewPoint.rotation = Quaternion.Euler(Mathf.Clamp(_verticalRotStore, -60f, 60f),
                    viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(Mathf.Clamp(-_verticalRotStore, -60f, 60f),
                    viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }

            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift))
            {
                _activeMoveSpeed = runSpeed;

                if (!footstepFast.isPlaying && moveDir == Vector3.zero)
                {
                    footstepFast.Play();
                    footstepSlow.Stop();
                }
            }
            else
            {
                _activeMoveSpeed = moveSpeed;
                
                if (!footstepSlow.isPlaying && moveDir == Vector3.zero)
                {
                    footstepSlow.Play();
                    footstepFast.Stop();
                }
            }

            if (moveDir == Vector3.zero || !IsGrounded())
            {
                footstepSlow.Stop();
                footstepFast.Stop();
            }

            float yVel = movement.y;
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * _activeMoveSpeed;
            movement.y = yVel;

            if (characterCon.isGrounded)
            {
                movement.y = 0.0f;
            }

            movement.y += Physics.gravity.y * gravityMyltiplier * Time.deltaTime;
            
            
            
            //isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.27f, groundLayer);
            //Debug.DrawRay(groundCheckPoint.position, Vector3.down * 0.27f, Color.red);
            //Debug.DrawLine(groundCheckPoint.position, new Vector3(0,0,-0.27f ), Color.yellow);
            
            
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                movement.y = jumpForse;
            }

            characterCon.Move(movement * Time.deltaTime);


            if (allWeapons[_selectedGun].muzzleFlash.activeInHierarchy)
            {
                _muzzleCount -= Time.deltaTime;
                if (_muzzleCount <= 0)
                {
                    allWeapons[_selectedGun].muzzleFlash.SetActive(false);
                }
            }

            if (!_overHeated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }

                if (Input.GetMouseButton(0) && allWeapons[_selectedGun].isAutomatic)
                {
                    _shootsCount -= Time.deltaTime;

                    if (_shootsCount <= 0)
                    {
                        Shoot();
                    }
                }

                _heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                _heatCounter -= overHeatCoolRate * Time.deltaTime;
                if (_heatCounter <= 0)
                {
                    _overHeated = false;
                    UIManager.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }

            if (_heatCounter < 0)
            {
                _heatCounter = 0;
            }

            UIManager.instance.orevheatedSlider.value = _heatCounter;

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                _selectedGun++;

                if (_selectedGun >= allWeapons.Length)
                {
                    _selectedGun = 0;
                }

                //SwitchWeapon();
                photonView.RPC("SetWeapon", RpcTarget.All, _selectedGun);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                _selectedGun--;
                if (_selectedGun < 0)
                {
                    _selectedGun = allWeapons.Length - 1;
                }

                //SwitchWeapon();
                photonView.RPC("SetWeapon", RpcTarget.All, _selectedGun);
            }

            
            
            for (int i = 0; i < allWeapons.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    _selectedGun = i;
                    //SwitchWeapon();
                    photonView.RPC("SetWeapon", RpcTarget.All, _selectedGun);
                }
            }

            anim.SetBool("grounded", IsGrounded());
            anim.SetFloat("speed", moveDir.magnitude);


            if (Input.GetMouseButton(1))
            {
                cam.fieldOfView = Mathf.Lerp( 
                    cam.fieldOfView, 
                    allWeapons[_selectedGun].adsZoom, 
                    adsSpeed * Time.deltaTime
                    );
                
                gunHolder.position = Vector3.Lerp(
                    gunHolder.position, 
                    adsInPoint.position, 
                    adsSpeed * Time.deltaTime
                    );
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp( 
                    cam.fieldOfView, 
                    60.0f, 
                    adsSpeed * Time.deltaTime
                );
                
                gunHolder.position = Vector3.Lerp(
                    gunHolder.position, 
                    adsOutPoint.position, 
                    adsSpeed * Time.deltaTime
                );
            }
            

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0) && !UIManager.instance.pauseMenu.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC(
                    "DealDamage", 
                    RpcTarget.All, 
                    photonView.Owner.NickName, 
                    allWeapons[_selectedGun].shootDamage,
                    PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletPref = Instantiate(bulletImpactPrefab, hit.point + (hit.normal * 0.002f ), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletPref, 1.2f);
            }
             
        }

        _shootsCount = allWeapons[_selectedGun].fireRate;

        _heatCounter += allWeapons[_selectedGun].heatPerShoot;
        if (_heatCounter >= maxHeat)
        {
            _heatCounter = maxHeat;

            _overHeated = true;
            UIManager.instance.overheatedMessage.gameObject.SetActive(true);
        }

        _muzzleCount = muzzleDisplayTime;
        allWeapons[_selectedGun].muzzleFlash.SetActive(true);

        
        allWeapons[_selectedGun].audioSource.Stop();
        allWeapons[_selectedGun].audioSource.Play();
    }

    private bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(groundCheckPoint.position + (transform.forward * 0.24f), Vector3.down),
            new Ray(groundCheckPoint.position + (-transform.forward * 0.24f), Vector3.down),
            new Ray(groundCheckPoint.position + (transform.right * 0.24f), Vector3.down),
            new Ray(groundCheckPoint.position + (-transform.right * 0.24f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++) 
        {
            if (Physics.Raycast(rays[i], 0.27f, groundLayer))
            {
                return true;
            }
        }

        return false;
    }

    [PunRPC]
    public void DealDamage(string damager, float dmgAmount, int actor)
    {
        
        TakeDamage(damager, dmgAmount, actor);
    }

    public void TakeDamage(string damager, float dmgAmount, int actor)
    {
        if (photonView.IsMine)
        {
            currentHealth -= dmgAmount;
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                PlayerSpawner.instance.Die(damager);
                
                MatchManager.instance.UpdateStatsSend(actor, 0,1);
            }
            
            UIManager.instance.currentHealthSlider.value = currentHealth;
        }
        
        
    }
    
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.instance.state == MatchManager.GameState.Playing)
            {
                cam.transform.position = viewPoint.position;
                cam.transform.rotation = viewPoint.rotation;
            }
            else
            {
                cam.transform.position = MatchManager.instance.mapCamPoint.position;
                cam.transform.rotation = MatchManager.instance.mapCamPoint.rotation;
            }
        }
        
    }

    private void SwitchWeapon()
    {
        foreach (Weapon gun in allWeapons)
        {
            gun.gameObject.SetActive(false);
        }

        allWeapons[_selectedGun].gameObject.SetActive(true);
        allWeapons[_selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void SetWeapon(int WeaponToSwitchTo)
    {
        if (WeaponToSwitchTo < allWeapons.Length)
        {
            _selectedGun = WeaponToSwitchTo;
            SwitchWeapon();
        }
    }
}
