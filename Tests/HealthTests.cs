using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HealthTests
{
    GameObject playerGO;
    GameObject zombieGO;
    ZombieAI zombieAI;
    ZombieDeath zombieDeath;

    [SetUp]
    public void Setup()
    {
        GlobalHealth.currentHealth = 100; // Reset máu player

        // Setup Player
        playerGO = new GameObject("PlayerHealth_Test");
        playerGO.tag = "Player";
        playerGO.transform.position = Vector3.zero;
        playerGO.AddComponent<CapsuleCollider>().isTrigger = false;

        // Setup Zombie (với AI và Death)
        zombieGO = new GameObject("ZombieHealth_Test");
        zombieGO.transform.position = new Vector3(0, 0, 1.5f); // Đặt gần để dễ test tấn công
        zombieGO.AddComponent<BoxCollider>().isTrigger = true; // Trigger cho AI
        Rigidbody zombieRb = zombieGO.AddComponent<Rigidbody>();
        zombieRb.isKinematic = true;
        zombieRb.useGravity = false;

        zombieAI = zombieGO.AddComponent<ZombieAI>();
        zombieAI.thePlayer = playerGO;
        zombieAI.theEnemy = zombieGO;
        zombieAI.enemySpeed = 0.01f;
        // Mock dependencies cho ZombieAI
        GameObject flashMock = new GameObject("FlashMock_HealthTest");
        flashMock.transform.SetParent(zombieGO.transform);
        zombieAI.theFlash = flashMock;
        flashMock.SetActive(false);
        zombieAI.hurtSound1 = zombieGO.AddComponent<AudioSource>();
        zombieAI.hurtSound2 = zombieGO.AddComponent<AudioSource>();
        zombieAI.hurtSound3 = zombieGO.AddComponent<AudioSource>();
        zombieGO.AddComponent<Animation>(); // Dependency cho ZombieAI và ZombieDeath

        zombieDeath = zombieGO.AddComponent<ZombieDeath>();
        zombieDeath.EnemyHealth = 50;
        zombieDeath.TheEnemy = zombieGO;
        zombieDeath.JumpscareMusic = zombieGO.AddComponent<AudioSource>();
        zombieDeath.AmbMusic = zombieGO.AddComponent<AudioSource>();
    }

    [TearDown]
    public void Teardown()
    {
        if (playerGO != null) Object.Destroy(playerGO);
        if (zombieGO != null) Object.Destroy(zombieGO);
        GlobalHealth.currentHealth = 20; // Reset về mặc định gốc
    }

    // --- TC_HEALTH_001 (Test bị Zombie tấn công mất máu) ---
    [UnityTest]
    public IEnumerator TC_HEALTH_001_TakingDamageFromZombie_DecreasesPlayerHealth()
    {
        int initialHealth = GlobalHealth.currentHealth;
        // Đảm bảo player đủ gần để trigger tấn công
        playerGO.transform.position = zombieGO.transform.position + zombieGO.transform.forward * 0.5f;

        // Đợi TriggerEnter và Coroutine InflictDamage chạy xong (~2.1s + buffer)
        yield return new WaitForSeconds(3.0f);

        // Assertion
        Assert.IsTrue(zombieAI.attackTrigger, "Zombie attack trigger should be active.");
        Assert.Less(GlobalHealth.currentHealth, initialHealth, "Player health should decrease after being attacked.");
        Assert.AreEqual(initialHealth - 5, GlobalHealth.currentHealth, "Player health should decrease by exactly 5 (default zombie damage).");
    }

    // --- TC_HEALTH_002 (Test chết khi hết máu) ---
    [UnityTest]
    public IEnumerator TC_HEALTH_002_PlayerDiesWhenHealthReachesZero()
    {
        GlobalHealth.currentHealth = 5; // Set máu rất thấp
        int initialHealth = GlobalHealth.currentHealth;

        // Action: Giả lập bị tấn công hoặc trừ máu trực tiếp
        // Cách 1: Để Zombie tấn công (như test trên)
        playerGO.transform.position = zombieGO.transform.position + zombieGO.transform.forward * 0.5f;
        yield return new WaitForSeconds(3.0f); // Đợi tấn công

        // Cách 2: Trừ máu trực tiếp (đơn giản hơn)
        // GlobalHealth.DeductHealth(initialHealth + 1); // Trừ nhiều hơn máu hiện có
        // yield return null; // Đợi 1 frame để Update() của GlobalHealth chạy

        // --- Assertion ---
        // Kiểm tra máu bằng 0
        Assert.AreEqual(0, GlobalHealth.currentHealth, "Player health should be 0 after fatal damage.");

        // **Cảnh báo:** Test này có thể bị gián đoạn bởi SceneManager.LoadScene("GameOver").
        // Nếu test bị lỗi ở đây, đó là do việc load scene.
        // Để test đúng kịch bản chết, cần xử lý việc load scene (nâng cao)
        // hoặc chấp nhận rằng test chỉ kiểm tra được trạng thái máu về 0.
        Debug.LogWarning("Test TC_HEALTH_002 might be interrupted if 'GameOver' scene loads automatically.");
    }

    // --- TC_HEALTH_003 (Test hồi máu) ---
    [Test]
    public void TC_HEALTH_003_Healing_NotImplemented()
    {
        // Bỏ qua vì chưa có logic/script hồi máu được cung cấp.
        Assert.Ignore("Skipping test: Healing logic/item script not provided.");
    }
}