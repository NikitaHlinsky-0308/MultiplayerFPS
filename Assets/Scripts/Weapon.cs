using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isAutomatic = true;
    public float fireRate = 0.1f, heatPerShoot = 1;
    public GameObject muzzleFlash;
    public float shootDamage;
    public float adsZoom;
    public AudioSource audioSource;
}
