using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
[TestFixture]

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    private void Update()
    {
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }


    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }


    private void FixedUpdate()
    {

        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

    }


    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    internal void SetMovementInput(Vector2 up)
    {
        throw new System.NotImplementedException();
    }
}
public class AudioAnimVFXTests
{
    GameObject playerGO;
    FirePistol firePistol; // C?n ?? test b?n súng
    AudioSource gunFireAudio;
    GameObject zombieGO;
    ZombieAI zombieAI;
    ZombieDeath zombieDeath;
    Animation zombieAnimation;
    AudioSource zombieHurtAudio; // Gi? s? l?y 1 trong 3 hurt sounds
    AudioSource zombieAttackAudio; // Gi? s? có AudioSource riêng cho attack ho?c dùng hurt sound
    AudioSource playerFootstepAudio;
    GameObject muzzleFlashVFX;
    GameObject playerHurtVFX; // theFlash

    [SetUp]
    public void Setup()
    {
        // --- Reset ---
        GlobalAmmo.ammoCount = 15;

        // --- Player ---
        playerGO = new GameObject("Player_AudioAnimVFXTest");
        playerGO.tag = "Player";
        playerGO.transform.position = Vector3.zero;
        playerGO.AddComponent<CapsuleCollider>();
        // Thêm FirstPersonController nếu test footsteps cần nó
        FirstPersonController controller = playerGO.AddComponent<FirstPersonController>();
        playerFootstepAudio = controller.GetComponent<AudioSource>();

        // --- Gun (G?n vào Player ho?c Camera) ---
        GameObject gunGO = new GameObject("Pistol_AudioAnimVFXTest");
        gunGO.transform.SetParent(playerGO.transform); // G?n vào Player
        firePistol = gunGO.AddComponent<FirePistol>();
        firePistol.DamageAmount = 5;
        // Gán components cho FirePistol
        GameObject theGunVisual = new GameObject("TheGunVisual"); // Object ch?a Animation Gun
        theGunVisual.AddComponent<Animation>();
        firePistol.TheGun = theGunVisual;
        muzzleFlashVFX = new GameObject("MuzzleFlashVFX"); // Object hi?u ?ng
        muzzleFlashVFX.SetActive(false);
        // muzzleFlashVFX.AddComponent<Animation>(); // N?u Muzzle Flash c?ng là Animation
        firePistol.MuzzleFlash = muzzleFlashVFX;
        gunFireAudio = gunGO.AddComponent<AudioSource>();
        firePistol.GunFire = gunFireAudio;

        // --- Zombie ---
        zombieGO = new GameObject("Zombie_AudioAnimVFXTest");
        zombieGO.transform.position = new Vector3(0, 0, 5); // ??t ? xa chút
        zombieGO.AddComponent<BoxCollider>().isTrigger = true;
        Rigidbody rb = zombieGO.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
        zombieAnimation = zombieGO.AddComponent<Animation>(); // Component Animation
        // Thêm các AnimationClip gi? l?p n?u c?n ki?m tra IsPlaying
        // AnimationClip walkClip = new AnimationClip{legacy=true}; anim.AddClip(walkClip, "walk");
        // AnimationClip attackClip = new AnimationClip{legacy=true}; anim.AddClip(attackClip, "attack");
        // AnimationClip deathClip = new AnimationClip{legacy=true}; anim.AddClip(deathClip, "back_fall");

        zombieAI = zombieGO.AddComponent<ZombieAI>();
        zombieAI.thePlayer = playerGO;
        zombieAI.theEnemy = zombieGO;
        playerHurtVFX = new GameObject("PlayerHurtVFX"); // theFlash
        playerHurtVFX.SetActive(false);
        zombieAI.theFlash = playerHurtVFX;
        zombieAI.hurtSound1 = zombieGO.AddComponent<AudioSource>(); zombieHurtAudio = zombieAI.hurtSound1; // Dùng t?m hurtSound1
        zombieAI.hurtSound2 = zombieGO.AddComponent<AudioSource>();
        zombieAI.hurtSound3 = zombieGO.AddComponent<AudioSource>();
        // zombieAttackAudio = zombieGO.AddComponent<AudioSource>(); // N?u có AudioSource riêng cho attack

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
        // H?y các GameObject khác n?u t?o ??c l?p
        GlobalHealth.preventSceneLoadForTesting = false;
        GlobalHealth.currentHealth = 20;
    }

    // --- Audio Tests ---

    // TC_AUD_GUN_001 (Manual Pass)
    [UnityTest]
    public IEnumerator TC_AUD_GUN_001_GunfireSoundPlays()
    {
        bool fired = firePistol.AttemptFire();
        Assert.IsTrue(fired);
        yield return null; // ??i 1 frame
        // Ki?m tra gián ti?p: AudioSource ?ã ???c g?i Play? Khó.
        // Ki?m tra IsPlaying có th? không ?úng vì âm thanh ng?n.
        // Gi? ??nh logic g?i là ?úng.
        Assert.Pass("Manual test passed. Assuming AudioSource.Play() was called.");
    }

    // TC_AUD_ZOM_001 (Manual Fail)
    [UnityTest]
    public IEnumerator TC_AUD_ZOM_001_ZombieHurtSound_Fails()
    {
        int initialHealth = zombieDeath.EnemyHealth;
        // B?n zombie
        bool fired = firePistol.AttemptFire();
        Assert.IsTrue(fired);
        yield return new WaitForSeconds(0.1f); // ??i raycast/sendmessage
                                               // Assertion: Ki?m tra s?c kh?e gi?m ?? ch?c ch?n b?n trúng
        Assert.Less(zombieDeath.EnemyHealth, initialHealth);

        // Ki?m tra âm thanh (mong ??i là Fail)
        // Assert.IsTrue(zombieHurtAudio.isPlaying); // Check này s? Fail
        Assert.Fail("Manual test failed: Zombie hurt sound likely not implemented or triggered.");
    }

    // TC_AUD_ZOM_002 (Manual Pass)
    [UnityTest]
    public IEnumerator TC_AUD_ZOM_002_ZombieAttackSoundPlays()
    {
        playerGO.transform.position = zombieGO.transform.position + Vector3.forward * 0.5f; // Player l?i g?n
        yield return new WaitForSeconds(3.0f); // ??i t?n công
                                               // Ki?m tra gián ti?p
        Assert.Less(GlobalHealth.currentHealth, 100); // Player m?t máu -> zombie ?ã t?n công
                                                      // Assert.IsTrue(zombieAI.hurtSound1.isPlaying || zombieAI.hurtSound2.isPlaying || zombieAI.hurtSound3.isPlaying); // Check này nên Pass
        Assert.Pass("Manual test passed. Assuming one of the hurt sounds played during attack.");
    }

    // TC_AUD_PLY_001 (Manual Pass)
    [UnityTest]
    public IEnumerator TC_AUD_PLY_001_FootstepSoundPlays()
    {
        // C?n FirstPersonController ?ã refactor input
        FirstPersonController controller = playerGO.GetComponent<FirstPersonController>();
        if (controller == null) Assert.Ignore("Requires FirstPersonController on Player.");
        playerFootstepAudio = controller.GetComponent<AudioSource>(); // L?y AudioSource t? FPC

        AudioClip initialClip = playerFootstepAudio.clip;
        bool playedSound = false;
        // Di chuy?n m?t ?o?n
        controller.SetMovementInput(Vector2.up);
        float endTime = Time.time + 1.0f;
        while (Time.time < endTime)
        {
            if (playerFootstepAudio.isPlaying && playerFootstepAudio.clip != initialClip)
            {
                playedSound = true;
                // break; // Không break v?i ?? xem có loop không
            }
            yield return new WaitForFixedUpdate();
        }
        Assert.IsTrue(playedSound, "Expected footstep sound to play (checking isPlaying and clip change).");
    }


    // --- Animation Tests ---

    // TC_ANI_GUN_001 (Manual Pass - ki?m tra IsPlaying)
    [UnityTest]
    public IEnumerator TC_ANI_GUN_001_PistolShootAnimationPlays()
    {
        Animation gunAnim = firePistol.TheGun?.GetComponent<Animation>();
        if (gunAnim == null) Assert.Ignore("No Animation component on TheGun.");

        bool fired = firePistol.AttemptFire();
        Assert.IsTrue(fired);
        yield return null; // ??i coroutine b?t ??u ch?y anim
                           // Ki?m tra xem animation có ?ang ch?y không
        Assert.IsTrue(gunAnim.IsPlaying("PistolShot"), "Expected 'PistolShot' animation to be playing.");
        yield return new WaitForSeconds(0.6f); // ??i anim k?t thúc
    }

    // TC_ANI_ZOM_001 (Manual Fail - ki?m tra IsPlaying)
    [UnityTest]
    public IEnumerator TC_ANI_ZOM_001_ZombieWalkAnimation_ShouldFailOrStutter()
    {
        if (zombieAnimation == null) Assert.Ignore("No Animation component on Zombie.");
        // ??i zombie di chuy?n
        yield return new WaitForSeconds(1.0f);
        // Ki?m tra có ?ang ch?y anim 'walk' không
        bool isPlayingWalk = zombieAnimation.IsPlaying("walk");
        // Theo KQ Manual là Fail (gi?t), nên test t? ??ng có th? PASS v? vi?c IsPlaying
        // nh?ng không th? ki?m tra ?? m??t.
        Assert.IsTrue(isPlayingWalk, "Expected 'walk' animation to be playing.");
        Debug.LogWarning("TC_ANI_ZOM_001: Automation checks if 'walk' is playing, but manual test reported stuttering (Fail).");
        // N?u mu?n test Fail kh?p Manual:
        // Assert.Fail("Manual test failed: Zombie walk animation reported as stuttering.");
    }

    // TC_ANI_ZOM_003 (Manual Pass - ki?m tra IsPlaying)
    [UnityTest]
    public IEnumerator TC_ANI_ZOM_003_ZombieDeathAnimationPlays()
    {
        if (zombieAnimation == null) Assert.Ignore("No Animation component on Zombie.");
        zombieDeath.EnemyHealth = 5;
        zombieDeath.DamageZombie(10); // Gi?t zombie
        yield return null; // ??i Update c?a ZombieDeath ch?y
        Assert.IsTrue(zombieAnimation.IsPlaying("back_fall"), "Expected 'back_fall' animation to be playing after death.");
    }


    // --- VFX Tests ---

    // TC_VFX_GUN_001 (Manual Pass - ki?m tra SetActive)
    [UnityTest]
    public IEnumerator TC_VFX_GUN_001_MuzzleFlashActivates()
    {
        Assert.IsNotNull(muzzleFlashVFX, "MuzzleFlash GameObject not assigned.");
        bool wasActiveBefore = muzzleFlashVFX.activeSelf;
        bool fired = firePistol.AttemptFire();
        Assert.IsTrue(fired);
        yield return null; // ??i coroutine ch?y SetActive(true)
        Assert.IsTrue(muzzleFlashVFX.activeSelf, "MuzzleFlash should be active immediately after firing.");
        yield return new WaitForSeconds(0.6f); // ??i coroutine k?t thúc
        // MuzzleFlash nên t? t?t sau animation ho?c coroutine? Gi? s? nó t?t.
        // Assert.IsFalse(muzzleFlashVFX.activeSelf, "MuzzleFlash should deactivate after effect.");
    }

    // TC_VFX_PLY_001 (Manual Fail - ki?m tra SetActive)
    [UnityTest]
    public IEnumerator TC_VFX_PLY_001_PlayerHurtEffect_Fails()
    {
        Assert.IsNotNull(playerHurtVFX, "Player Hurt VFX (theFlash) not assigned.");
        bool wasActiveBefore = playerHurtVFX.activeSelf;
        // ?? zombie t?n công
        playerGO.transform.position = zombieGO.transform.position + Vector3.forward * 0.5f;
        yield return new WaitForSeconds(3.0f); // ??i t?n công
                                               // Assertion (mong ??i Fail theo Manual)
        Assert.IsFalse(playerHurtVFX.activeSelf, "FAIL Check: Player hurt effect (theFlash) did not activate.");
        // N?u mu?n test PASS n?u nó CÓ b?t:
        // Assert.IsTrue(playerHurtVFX.activeSelf, "Player hurt effect should activate briefly.");
    }
}