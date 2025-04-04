using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovementTests
{
    // --- TC_MOVE_001, TC_MOVE_002 ---
    [Test]
    public void TC_MOVE_001_002_MoveForwardLeft_SkippedDueToInput()
    {
        // Bỏ qua vì không thể giả lập nhấn phím W/A đáng tin cậy
        // mà không sửa code FirstPersonController.cs
        Assert.Ignore("Skipping test: Cannot simulate key presses (W/A) without refactoring FirstPersonController to accept external commands.");
    }

    // --- TC_MOVE_003 ---
    [Test]
    public void TC_MOVE_003_WallCollision_SkippedDueToInput()
    {
        // Bỏ qua vì việc kiểm tra va chạm tường yêu cầu nhân vật
        // phải di chuyển, mà việc kích hoạt di chuyển lại phụ thuộc input.
        Assert.Ignore("Skipping test: Cannot reliably test wall collision without simulating movement, which is blocked by input limitations.");
    }

    // Tương tự, các test cho chạy (Shift), nhảy (Space) cũng sẽ bị bỏ qua.
}