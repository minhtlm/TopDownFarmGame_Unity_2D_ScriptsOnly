using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tst : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public Vector2 boxSize = new Vector3(0.8f, 0.4f); // Kích thước giống OverlapBox
    public Vector2 lookDirection = Vector2.right; // Hướng nhân vật đang nhìn

    private void OnDrawGizmos()
    {
        if (rb2d == null) return;

        // Tính toán vị trí hộp giống với OverlapBox
        // Vector2 boxCenter = rb2d.position + new Vector2(lookDirection.x * 1.0f, 0.2f);

        // Chọn màu (xanh lá) để dễ nhìn
        Gizmos.color = Color.green; 

        // Vẽ hộp trong Scene View
        // Gizmos.DrawWireCube(boxCenter, boxSize);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.5f, 0), 1.0f);
    }
}
