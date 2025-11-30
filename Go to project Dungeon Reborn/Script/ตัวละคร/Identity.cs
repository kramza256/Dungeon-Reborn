using UnityEngine;

public class Identity : MonoBehaviour
{
    [SerializeField]
    string _name;
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(_name))
            {
                _name = gameObject.name;
            }
            else
            {
                gameObject.name = _name;
            }
            return _name;
        }
        set { _name = value; }
    }

    public int positionX
    {
        get { return Mathf.RoundToInt(transform.position.x); }
        set
        {
            transform.position = new Vector3(value, transform.position.y, transform.position.z);
        }
    }

    public int positionY
    {
        get { return Mathf.RoundToInt(transform.position.z); }
        set
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, value);
        }
    }

    // ✅ แก้ไข: เปลี่ยน property ให้มี set; หรือใช้ field ปกติ เพื่อให้คลาสอื่นกำหนดค่าได้
    private Player _player;
    public Player player
    {
        get
        {
            if (_player == null)
            {
                _player = FindFirstObjectByType<Player>(); // Unity 6 ใช้ FindFirstObjectByType
                if (_player == null)
                {
                    // Debug.LogWarning("No Player found in the scene.");
                }
            }
            return _player;
        }
        set { _player = value; } // ✅ เพิ่ม setter
    }

    private float distanFormPlayer;

    private GameObject _IdentityInFront;

    // Property นี้จะค้นหาวัตถุที่อยู่ข้างหน้า
    public Identity InFront
    {
        get
        {
            RaycastHit hit = GetClosestInfornt();
            if (hit.collider != null)
            {
                _IdentityInFront = hit.collider.gameObject;
                return _IdentityInFront.GetComponent<Identity>();
            }
            else
            {
                // ไม่เจออะไรข้างหน้า
                _IdentityInFront = null;
                // Debug.LogWarning("Not found InFront Identity."); // ปิดได้ถ้าไม่อยากให้รก Console
                return null;
            }
        }
    }

    // กำหนดรัศมีและระยะทางตามที่ใช้ในคุณสมบัติ InFront
    float sphereRadius = 0.5f;
    float maxDistance = 1f;

    private void Start()
    {
        SetUP();
    }

    public virtual void SetUP()
    {
        // Override ในคลาสลูกได้
    }

    protected float GetDistanPlayer()
    {
        if (player == null) return float.MaxValue;
        distanFormPlayer = Vector3.Distance(transform.position, player.transform.position);
        return distanFormPlayer;
    }

    public string GetInfo()
    {
        return ("Name : " + Name + " x:" + positionX + " y:" + positionY);
    }

    protected RaycastHit GetClosestInfornt()
    {
        // ใช้ SphereCast เพื่อหาของข้างหน้า
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sphereRadius, transform.forward, maxDistance);

        RaycastHit closestHit = new RaycastHit();
        float minDistance = float.MaxValue;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            // ต้องไม่ใช่ตัวเอง และต้องมี Identity
            if (hit.collider.gameObject != gameObject && hit.collider.GetComponent<Identity>() != null)
            {
                if (hit.distance < minDistance)
                {
                    minDistance = hit.distance;
                    closestHit = hit;
                    found = true;
                }
            }
        }

        // ถ้าไม่เจอ ให้คืนค่าว่าง (default RaycastHit จะมี collider เป็น null)
        if (!found) return new RaycastHit();

        return closestHit;
    }

    private void OnDrawGizmos()
    {
        Vector3 endPosition = transform.position + transform.forward * maxDistance;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(endPosition, sphereRadius);

        if (_IdentityInFront != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_IdentityInFront.transform.position, sphereRadius * 1.5f);
        }
    }
}