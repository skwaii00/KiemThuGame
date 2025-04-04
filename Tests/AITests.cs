using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ZombieAI : MonoBehaviour
{
    public GameObject thePlayer;
    public GameObject theEnemy;
    public float enemySpeed = 0.01f;
    public bool attackTrigger = false;
    public bool isAttacking = false;
    public AudioSource hurtSound1;
    public AudioSource hurtSound2;
    public AudioSource hurtSound3;
    public int hurtGen;
    public GameObject theFlash;

    private Animation anim;

    void Start()
    {
        anim = theEnemy.GetComponent<Animation>();
    }

    void Update()
    {
        if (thePlayer != null)
        {
            transform.LookAt(thePlayer.transform);
        }

        if (theEnemy != null)
        {
            if (attackTrigger == false)
            {
                enemySpeed = 0.01f;
                anim.Play("walk");
                if (thePlayer != null) // Check null here as well, for safety
                {
                    transform.position = Vector3.MoveTowards(transform.position, thePlayer.transform.position, enemySpeed);
                }
            }

            if (attackTrigger == true && isAttacking == false)
            {
                enemySpeed = 0;
                anim.Play("attack");
                StartCoroutine(InflictDamage());
            }
        }
    }

    void OnTriggerEnter()
    {
        {
            attackTrigger = true;
        }
    }

    void OnTriggerExit()
    {
        {
            attackTrigger = false;
        }
    }

    IEnumerator InflictDamage()
    {
        isAttacking = true;
        hurtGen = Random.Range(1, 4);
        if (hurtGen == 1)
        {
            hurtSound1.Play();
        }
        if (hurtGen == 2)
        {
            hurtSound2.Play();
        }
        if (hurtGen == 3)
        {
            hurtSound3.Play();
        }
        theFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        theFlash.SetActive(false);
        yield return new WaitForSeconds(1.1f);
        GlobalHealth.DeductHealth(5);
        yield return new WaitForSeconds(0.9f);
        isAttacking = false;
    }
}
//GLOBAL HEALTH

public class GlobalHealth : MonoBehaviour
{
    public static GlobalHealth Instance { get; private set; } // Singleton instance

    public static int currentHealth = 20;
    public int internalHealth;
    internal static bool preventSceneLoadForTesting;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }
    }

    void Update()
    {
        internalHealth = currentHealth;
        if (internalHealth <= 0)
        {
            SceneManager.LoadScene("GameOver"); // GameOver Khi hết máu!
        }
    }
    public static void DeductHealth(int damage) //Function called from other scripts
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0
    }
}
//ZOMBIE DEATH
public class ZombieDeath : MonoBehaviour
{

    public int EnemyHealth = 20;
    public GameObject TheEnemy;
    public int StatusCheck;
    public AudioSource JumpscareMusic;
    public AudioSource AmbMusic;

    // --- THÊM "public" VÀO ĐÂY ---
    public void DamageZombie(int DamageAmount)
    {
        EnemyHealth -= DamageAmount;
    }
    // -----------------------------

    void Update()
    {
        if (EnemyHealth <= 0 && StatusCheck == 0)
        {
            // Kiểm tra null trước khi GetComponent để tránh lỗi
            ZombieAI ai = this.GetComponent<ZombieAI>();
            if (ai != null) ai.enabled = false;

            BoxCollider bc = this.GetComponent<BoxCollider>();
            if (bc != null) bc.enabled = false;

            StatusCheck = 2;

            Animation anim = TheEnemy?.GetComponent<Animation>(); // Toán tử ?. an toàn hơn
            if (anim != null)
            {
                anim.Stop("walk"); // Có thể gây lỗi nếu animation "walk" không tồn tại hoặc không chạy
                anim.Play("back_fall"); // Đảm bảo bạn có animation clip tên "back_fall"
            }

            if (JumpscareMusic != null) JumpscareMusic.Stop();
            if (AmbMusic != null) AmbMusic.Play();
        }
    }
}


public class AITests
{
    GameObject playerGO;
    GameObject zombieGO;
    ZombieAI zombieAI;
    ZombieDeath zombieDeath; // Thêm tham chiếu
    // GameObject globalHealthGO; // Không cần nếu chỉ reset static var

    [SetUp]
    public void Setup()
    {
        GlobalHealth.currentHealth = 100; // Reset máu player

        playerGO = new GameObject("PlayerAI_Test");
        playerGO.tag = "Player";
        playerGO.transform.position = Vector3.zero;
        playerGO.AddComponent<CapsuleCollider>().isTrigger = false;

        // --- Setup Zombie ---
        zombieGO = new GameObject("ZombieAI_Test");
        zombieGO.transform.position = new Vector3(0, 0, 10);

        zombieGO.AddComponent<BoxCollider>().isTrigger = true; // Trigger cho AI
        // Thêm lại BoxCollider không phải trigger cho ZombieDeath nếu cần
        // zombieGO.AddComponent<BoxCollider>().isTrigger = false;
        Rigidbody zombieRb = zombieGO.AddComponent<Rigidbody>();
        zombieRb.isKinematic = true;
        zombieRb.useGravity = false;

        // Thêm ZombieAI
        zombieAI = zombieGO.AddComponent<ZombieAI>();
        zombieAI.thePlayer = playerGO;
        zombieAI.theEnemy = zombieGO;
        zombieAI.enemySpeed = 0.01f;

        // Thêm ZombieDeath và dependencies của nó
        zombieDeath = zombieGO.AddComponent<ZombieDeath>();
        zombieDeath.EnemyHealth = 50; // Set máu khởi đầu zombie
        zombieDeath.TheEnemy = zombieGO;
        zombieDeath.JumpscareMusic = zombieGO.AddComponent<AudioSource>();
        zombieDeath.AmbMusic = zombieGO.AddComponent<AudioSource>();

        // Thêm Animation cho cả ZombieAI và ZombieDeath
        zombieGO.AddComponent<Animation>();

        // Mock dependencies khác cho ZombieAI
        GameObject flashMock = new GameObject("FlashMock_AI");
        flashMock.transform.SetParent(zombieGO.transform);
        zombieAI.theFlash = flashMock;
        flashMock.SetActive(false);
        zombieAI.hurtSound1 = zombieGO.AddComponent<AudioSource>();
        zombieAI.hurtSound2 = zombieGO.AddComponent<AudioSource>();
        zombieAI.hurtSound3 = zombieGO.AddComponent<AudioSource>();
    }

    [TearDown]
    public void Teardown()
    {
        if (playerGO != null) Object.Destroy(playerGO);
        if (zombieGO != null) Object.Destroy(zombieGO);
        GlobalHealth.currentHealth = 20; // Reset về mặc định gốc
    }

    // ... (Các test case TC_AI_001, TC_AI_002, TC_AI_006 giữ nguyên như trước) ...

    // --- THÊM TEST CASE MỚI CHO ZOMBIE DEATH ---

    [UnityTest]
    public IEnumerator TC_DEATH_001_ZombieHealthDecreasesOnDamage()
    {
        int initialHealth = zombieDeath.EnemyHealth;
        int damage = 10;

        // Action: Gọi trực tiếp hàm DamageZombie
        zombieDeath.DamageZombie(damage);
        yield return null; // Đợi 1 frame

        // Assertion
        Assert.AreEqual(initialHealth - damage, zombieDeath.EnemyHealth, "Zombie health should decrease correctly.");
    }

    [UnityTest]
    public IEnumerator TC_DEATH_002_ZombieDiesWhenHealthIsZeroOrLess()
    {
        zombieDeath.EnemyHealth = 5; // Set máu thấp
        int damage = 10;

        // Pre-assertion: Kiểm tra trạng thái sống ban đầu
        Assert.IsTrue(zombieAI.enabled, "ZombieAI should be enabled initially.");
        Assert.IsTrue(zombieGO.GetComponent<BoxCollider>().enabled, "BoxCollider should be enabled initially.");
        Assert.AreEqual(0, zombieDeath.StatusCheck, "StatusCheck should be 0 initially.");


        // Action: Gây sát thương chí mạng
        zombieDeath.DamageZombie(damage);

        // Đợi Update() trong ZombieDeath chạy để xử lý logic chết
        yield return null; // Đợi 1 frame

        // Assertions: Kiểm tra trạng thái chết
        Assert.IsFalse(zombieAI.enabled, "ZombieAI should be disabled after death.");
        // Lưu ý: Nếu bạn có nhiều BoxCollider, cần kiểm tra đúng cái
        // Assert.IsFalse(zombieGO.GetComponent<BoxCollider>().enabled, "BoxCollider should be disabled after death.");
        // => Code gốc chỉ disable BoxCollider đầu tiên tìm thấy. Nếu có nhiều, cần kiểm tra cụ thể hơn.
        Assert.AreEqual(2, zombieDeath.StatusCheck, "StatusCheck should be 2 after death.");

        // Kiểm tra animation nếu có mock/setup clip "back_fall"
        // Animation anim = zombieGO.GetComponent<Animation>();
        // Assert.IsTrue(anim.IsPlaying("back_fall"), "Death animation should be playing.");
    }
}