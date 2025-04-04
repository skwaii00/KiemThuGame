using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class GlobalAmmo : MonoBehaviour
{

    public static int ammoCount;
    public GameObject ammoDisplay;
    public int internalAmmo;

    void Update()
    {
        internalAmmo = ammoCount;
        ammoDisplay.GetComponent<Text>().text = "" + ammoCount;
    }
}


public class AmmoPickup : MonoBehaviour
{



    public GameObject theAmmo;

    public GameObject ammoDisplayBox;



    void OnTriggerEnter(Collider other)

    {

        ammoDisplayBox.SetActive(true);

        GlobalAmmo.ammoCount += 7;

        theAmmo.SetActive(false);

    }



}
[TestFixture]
public class ItemTests
{
    GameObject playerGO;
    GameObject ammoPickupGO;
    GameObject globalAmmoGO;
    GameObject ammoDisplayBoxMock;
    GameObject theAmmoMock; // GameObject đại diện cho hình ảnh ammo
    AmmoPickup pickupScript; // Tham chiếu đến script thật

    [SetUp]
    public void Setup()
    {
        // Setup GlobalAmmo
        globalAmmoGO = new GameObject("GlobalAmmo_ItemTest");
        globalAmmoGO.AddComponent<GlobalAmmo>();
        GlobalAmmo.ammoCount = 5; // Bắt đầu với ít đạn

        // Setup Player (Quan trọng: cần Rigidbody và Collider không phải trigger)
        playerGO = new GameObject("Player_ItemTest");
        playerGO.tag = "Player"; // Giả sử pickup kiểm tra tag
        playerGO.transform.position = Vector3.zero;
        playerGO.AddComponent<CapsuleCollider>().isTrigger = false; // Collider vật lý
        Rigidbody rb = playerGO.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // Dùng kinematic để điều khiển vị trí chính xác

        // Setup Ammo Pickup
        ammoPickupGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ammoPickupGO.name = "AmmoBox_Test";
        ammoPickupGO.transform.position = new Vector3(0, 0, 2);
        Object.Destroy(ammoPickupGO.GetComponent<MeshRenderer>()); // Xóa hình ảnh cube mặc định

        // Collider của pickup PHẢI LÀ TRIGGER
        ammoPickupGO.AddComponent<BoxCollider>().isTrigger = true;

        // Gắn script AmmoPickup thật
        pickupScript = ammoPickupGO.AddComponent<AmmoPickup>();

        // Tạo và gán các GameObject giả lập cho dependencies
        ammoDisplayBoxMock = new GameObject("AmmoDisplayBoxMock");
        ammoDisplayBoxMock.SetActive(false); // Bắt đầu ẩn
        pickupScript.ammoDisplayBox = ammoDisplayBoxMock;

        theAmmoMock = new GameObject("TheAmmoVisualMock"); // Hình ảnh 3D giả
        theAmmoMock.transform.SetParent(ammoPickupGO.transform); // Làm con của pickup
        pickupScript.theAmmo = theAmmoMock;
    }

    [TearDown]
    public void Teardown()
    {
        // Dọn dẹp
        if (playerGO != null) Object.Destroy(playerGO);
        // ammoPickupGO có thể đã tự hủy nếu test thành công
        if (ammoPickupGO != null) Object.Destroy(ammoPickupGO);
        if (globalAmmoGO != null) Object.Destroy(globalAmmoGO);
        if (ammoDisplayBoxMock != null) Object.Destroy(ammoDisplayBoxMock);
        // theAmmoMock sẽ tự hủy khi ammoPickupGO bị hủy
    }

    // --- TC_ITEM_001 ---
    [UnityTest]
    public IEnumerator TC_ITEM_001_TriggeringAmmoPickup_IncreasesAmmoByCorrectAmount()
    {
        int initialAmmo = GlobalAmmo.ammoCount;
        int expectedAmmoIncrease = 7; // Theo code AmmoPickup.cs

        // --- Action ---
        // Di chuyển player vào trigger của pickup một cách từ từ
        Vector3 targetPos = ammoPickupGO.transform.position;
        Rigidbody playerRb = playerGO.GetComponent<Rigidbody>();
        float moveDuration = 0.5f;
        float startTime = Time.time;

        while (Time.time < startTime + moveDuration)
        {
            if (playerGO == null || ammoPickupGO == null) break; // Thoát nếu đối tượng bị hủy sớm
            // Di chuyển bằng Rigidbody.MovePosition
            playerRb.MovePosition(Vector3.Lerp(playerGO.transform.position, targetPos, (Time.time - startTime) / moveDuration));
            yield return new WaitForFixedUpdate(); // Đợi vật lý chạy sau khi di chuyển
        }

        // Đợi thêm một chút để OnTriggerEnter chắc chắn được gọi và xử lý
        yield return new WaitForSeconds(0.2f);

        // --- Assertion ---
        Assert.AreEqual(initialAmmo + expectedAmmoIncrease, GlobalAmmo.ammoCount, $"Ammo count should increase by {expectedAmmoIncrease}.");

        // Kiểm tra trạng thái các GameObject phụ (tùy chọn)
        Assert.IsTrue(ammoDisplayBoxMock.activeSelf, "Ammo Display Box should become active.");
        Assert.IsFalse(theAmmoMock.activeSelf, "The Ammo visual object should become inactive.");
        // Assert.IsTrue(ammoPickupGO == null, "Pickup object should be destroyed."); // Kiểm tra null có thể không đáng tin cậy ngay lập tức
    }

    // --- Các test item khác bị bỏ qua ---
    [Test] public void TC_ITEM_002_HealthPickup_NotImplemented() { Assert.Ignore("Skipping: Health pickup logic/script not provided."); }
    [Test] public void TC_ITEM_003_ItemDisplay_Skipped() { Assert.Ignore("Skipping: Testing UI/Inventory display requires specific setup."); }
    [Test] public void KeyPickup_Test_Skipped() { Assert.Ignore("Skipping: KeyPickup relies on OnMouseOver and Input."); }
    [Test] public void PistolPickup_Test_Skipped() { Assert.Ignore("Skipping: PickUpPistol relies on OnMouseOver and Input."); }
}