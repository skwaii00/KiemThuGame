using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI; // Chỉ cần using 1 lần ở đầu file

public class FirePistol : MonoBehaviour
{
    public GameObject TheGun;
    public GameObject MuzzleFlash;
    public AudioSource GunFire;
    public bool IsFiring = false;
    public float TargetDistance;
    public int DamageAmount = 5;

    // Update
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            AttemptFire(); // Gọi hàm public khi nhấn nút
        }
    }

    // --- Hàm Public để Test và Update gọi ---
    public bool AttemptFire()
    {
        if (!IsFiring && GlobalAmmo.ammoCount >= 1)
        {
            GlobalAmmo.ammoCount -= 1;
            StartCoroutine(FiringPistol()); // Gọi coroutine private từ đây
            return true;
        }
        // Debug.LogWarning($"AttemptFire failed: IsFiring={IsFiring}, Ammo={GlobalAmmo.ammoCount}"); //
        return false;
    }

    // Coroutine có thể giữ là private
    IEnumerator FiringPistol()
    {
        RaycastHit Shot;
        IsFiring = true;
        Transform firePoint = Camera.main != null ? Camera.main.transform : transform;

        if (Physics.Raycast(firePoint.position, firePoint.forward, out Shot))
        {
            TargetDistance = Shot.distance;
            Shot.transform.SendMessage("DamageZombie", DamageAmount, SendMessageOptions.DontRequireReceiver);
        }

        // Check null trước khi sử dụng components
        Animation gunAnim = TheGun?.GetComponent<Animation>();
        if (gunAnim != null) gunAnim.Play("PistolShot");

        if (MuzzleFlash != null)
        {
            MuzzleFlash.SetActive(true);
            Animation muzzleAnim = MuzzleFlash.GetComponent<Animation>();
            if (muzzleAnim != null) muzzleAnim.Play("MuzzleAnim");
        }

        if (GunFire != null) GunFire.Play();

        yield return new WaitForSeconds(0.5f);
        IsFiring = false;
    }
}

[TestFixture] // Thêm attribute này cho class test
public class ShootingTests
{
    GameObject playerGO;
    GameObject gunGO;
    FirePistol firePistol;
    GameObject targetGO; // Zombie
    ZombieDeath targetZombieDeath;
    GameObject ammoDisplayGO;
    GameObject globalAmmoGO;

    [SetUp]
    public void Setup()
    {
        // Setup GlobalAmmo
        globalAmmoGO = new GameObject("GlobalAmmo_TestSetup");
        GlobalAmmo ammoScript = globalAmmoGO.AddComponent<GlobalAmmo>();
        // Tạo UI giả
        ammoDisplayGO = new GameObject("AmmoDisplayMock");
        ammoDisplayGO.AddComponent<RectTransform>();
        Text textComp = ammoDisplayGO.AddComponent<Text>();
        // textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Gán font nếu cần
        if (ammoScript != null) ammoScript.ammoDisplay = ammoDisplayGO;
        GlobalAmmo.ammoCount = 15; // Reset đạn

        // Setup Player và Camera
        playerGO = new GameObject("PlayerShooter");
        playerGO.AddComponent<Camera>();
        playerGO.transform.position = Vector3.zero;

        // Setup Gun và FirePistol
        gunGO = new GameObject("Pistol");
        gunGO.transform.SetParent(playerGO.transform);
        firePistol = gunGO.AddComponent<FirePistol>();
        firePistol.DamageAmount = 5;
        // Gán dependencies giả lập cho FirePistol
        firePistol.MuzzleFlash = new GameObject("MuzzleFlashMock");
        firePistol.MuzzleFlash.SetActive(false);
        firePistol.GunFire = gunGO.AddComponent<AudioSource>();
        // firePistol.TheGun = gunGO; // Gán nếu cần
        // gunGO.AddComponent<Animation>();

        // Setup Target Zombie với ZombieDeath
        targetGO = new GameObject("TargetZombie_ShootingTest");
        targetGO.transform.position = new Vector3(0, 0, 10);
        targetGO.AddComponent<BoxCollider>();
        targetZombieDeath = targetGO.AddComponent<ZombieDeath>();
        targetZombieDeath.EnemyHealth = 50;
        targetZombieDeath.TheEnemy = targetGO;
        targetGO.AddComponent<ZombieAI>().enabled = false;
        targetGO.AddComponent<Animation>();
        targetZombieDeath.JumpscareMusic = targetGO.AddComponent<AudioSource>();
        targetZombieDeath.AmbMusic = targetGO.AddComponent<AudioSource>();
    }

    [TearDown]
    public void Teardown()
    {
        // Dọn dẹp
        if (targetGO != null) Object.Destroy(targetGO);
        if (playerGO != null) Object.Destroy(playerGO);
        if (globalAmmoGO != null) Object.Destroy(globalAmmoGO);
    }

    // --- TC_SHOOT_001 (Sửa lại cách gọi Action) ---
    [UnityTest]
    public IEnumerator TC_SHOOT_001_AttemptFire_DecreasesAmmoAndDamagesZombie()
    {
        int initialAmmo = GlobalAmmo.ammoCount;
        int initialZombieHealth = targetZombieDeath.EnemyHealth;
        Assert.Greater(initialAmmo, 0);

        // --- Action: Gọi hàm public AttemptFire ---
        bool fired = firePistol.AttemptFire();
        // ------------------------------------------

        // Đợi Coroutine FiringPistol hoàn thành nếu bắn thành công
        if (fired)
        {
            yield return new WaitForSeconds(0.6f); // Đợi hơn 0.5s
        }
        else
        {
            Assert.Fail("AttemptFire() should return true when ammo is available.");
        }

        // --- Assertion ---
        Assert.IsTrue(fired, "AttemptFire() return value should be true.");
        Assert.AreEqual(initialAmmo - 1, GlobalAmmo.ammoCount, "Ammo should decrease by 1.");
        Assert.AreEqual(initialZombieHealth - firePistol.DamageAmount, targetZombieDeath.EnemyHealth, "Zombie health should decrease by damage amount.");
        Assert.IsFalse(firePistol.IsFiring, "IsFiring flag should be reset after coroutine.");
    }

    // --- TC_SHOOT_002 ---
    [Test]
    public void TC_SHOOT_002_Reload_NotImplemented()
    {
        Assert.Ignore("Skipping test: Reload logic not implemented.");
    }

    // --- TC_SHOOT_003 ---
    [UnityTest]
    public IEnumerator TC_SHOOT_003_AttemptFire_FailsWithNoAmmo()
    {
        GlobalAmmo.ammoCount = 0; // Hết đạn
        int initialZombieHealth = targetZombieDeath.EnemyHealth;

        // --- Action: Gọi hàm public ---
        bool fired = firePistol.AttemptFire();
        // -----------------------------
        yield return null; // Đợi 1 frame

        // --- Assertion ---
        Assert.IsFalse(fired, "AttemptFire() should return false when no ammo.");
        Assert.AreEqual(0, GlobalAmmo.ammoCount, "Ammo should remain 0.");
        Assert.AreEqual(initialZombieHealth, targetZombieDeath.EnemyHealth, "Zombie health should not change.");
        Assert.IsFalse(firePistol.IsFiring, "IsFiring flag should remain false.");
    }
}