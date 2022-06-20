using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(GlowManager))]
//[RequireComponent(typeof(JengaController), typeof(ObjectControl))]
public class Block : MonoBehaviour
{
    public const float scale = 1.0f;
    public const float width = 1f;
    public const float height = 0.5f;
    public const float length = 3f;
    public const float deformation = 0.02f;
    public const float weight = 0.0f;

    private Mesh mesh;

    public int BlockType = 0;
    public int BlockIdx = 0;
    public bool IsNearestToDevice = false;
    public bool IsSelected = false;

    private Material initMaterial;
    public Material transMaterial;

    [Header("ArrowGuide")]
    public Transform CurveArrowPrefab;
    public Transform DirectArrowPrefab;
    public Transform OriginPrefab;
    public static bool IsUsingGuide = false;
    public static int GuidingBlockNum = -1;
    private List<Transform> arrowGuides;
    private Transform origin;
    public const float arrowDeformation = 0.5f;
    public Vector3 originVec;

    private Vector3 DeformRandomly(Vector3 point)
    {
        return new Vector3(
            Random.Range(point.x - deformation, point.x + deformation),
            Random.Range(point.y - deformation, point.y + deformation),
            Random.Range(point.z - deformation, point.z + deformation)
        );
    }

    private void InitMesh()
    {
        if (mesh != null)
        {
            return;
        }

        mesh = new Mesh();
        mesh.name = "Jenga block";

        var vertices = new Vector3[]
        {
            DeformRandomly(new Vector3(0f, 0f, 0f)),
            DeformRandomly(new Vector3(0f, height, 0f)),
            DeformRandomly(new Vector3(width, height, 0f)),
            DeformRandomly(new Vector3(width, 0f, 0f)),
            DeformRandomly(new Vector3(0f, 0f, length)),
            DeformRandomly(new Vector3(0f, height, length)),
            DeformRandomly(new Vector3(width, height, length)),
            DeformRandomly(new Vector3(width, 0f, length)),
        };

        // 블럭 축 변경
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x -= width / 2.0f;
            vertices[i].y -= height / 2.0f;
            vertices[i].z -= length;
        }

        var triangles = new int[] {
            0, 1, 2,
            2, 3, 0,

            7, 6, 5,
            5, 4, 7,

            1, 5, 6,
            6, 2, 1,

            4, 0, 3,
            3 ,7, 4,

            0, 4, 5,
            5, 1, 0,

            2, 6, 7,
            7, 3, 2,
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; ++i)
        {
            uvs[i] = new Vector2(vertices[i].x / mesh.bounds.size.x, vertices[i].z / mesh.bounds.size.z);
        }
        mesh.uv = uvs;

        GetComponent<MeshFilter>().mesh = mesh;

        var collider = GetComponent<MeshCollider>();

        collider.convex = true;
        collider.sharedMesh = mesh;

        Rigidbody body = GetComponent<Rigidbody>();
        body.mass = weight;

        initMaterial = GetComponent<MeshRenderer>().material;

        originVec = transform.position - new Vector3(transform.position.x, transform.position.y, transform.position.z - length);
    }

    private void OnDestroy()
    {
        foreach (var arrow in arrowGuides)
        {
            Destroy(arrow.gameObject);
        }
        Destroy(origin.gameObject);
    }
    private void InitArrow()
    {
        arrowGuides = new List<Transform>(new Transform[6]);
        Quaternion rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);

        Transform arrow = Instantiate(DirectArrowPrefab, transform);
        arrow.localPosition = new Vector3(0, 0, -length - arrowDeformation);
        arrow.localRotation = rotation;
        arrowGuides[0] = arrow;

        arrow = Instantiate(DirectArrowPrefab, transform);
        arrow.localPosition = new Vector3(-width, 0, -length / 2.0f);
        rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        arrow.localRotation = rotation;
        arrowGuides[1] = arrow;

        arrow = Instantiate(DirectArrowPrefab, transform);
        arrow.localPosition = new Vector3(0.0f, 0.0f, arrowDeformation);
        rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        arrow.localRotation = rotation;
        arrowGuides[2] = arrow;

        arrow = Instantiate(DirectArrowPrefab, transform);
        arrow.localPosition = new Vector3(width, 0, -length / 2.0f);
        rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        arrow.localRotation = rotation;
        arrowGuides[3] = arrow;

        arrow = Instantiate(CurveArrowPrefab, transform);
        arrow.localPosition = new Vector3(-width, 0, -length - arrowDeformation);
        rotation = Quaternion.Euler(270.0f, 45.0f, 0.0f);
        arrow.localRotation = rotation;
        arrowGuides[4] = arrow;

        arrow = Instantiate(CurveArrowPrefab, transform);
        arrow.localPosition = new Vector3(width, 0, -length - arrowDeformation);
        rotation = Quaternion.Euler(90.0f, 0.0f, -135.0f);
        arrow.localRotation = rotation;
        arrowGuides[5] = arrow;

        Transform originBlock = Instantiate(OriginPrefab, transform);
        originBlock.localPosition = new Vector3(0, 0, -length / 2.0f);
        origin = originBlock;
        origin.gameObject.SetActive(false);

        ArrowGuideOff();
    }

    private void Start()
    {
        InitMesh();
        InitArrow();
    }

    public void MakeObjectGlow()
    {
        IsNearestToDevice = true;
        GetComponent<GlowManager>().EnableGlow();
    }

    public void MakeObjectToOriginalState()
    {
        IsNearestToDevice = false;
        GetComponent<GlowManager>().DisableGlow();
    }


    public void ArrowGuideOn()
    {
        if (Block.IsUsingGuide) return;
        if (BlockIdx == Block.GuidingBlockNum) return;

        Ray ray = new Ray();
        RaycastHit rayHit;
        float distance = 10.0f;

        // x+ 방향 먼저 체크
        ray.origin = origin.position;
        ray.direction = arrowGuides[3].position - arrowGuides[1].position;
        if (Physics.Raycast(ray.origin, ray.direction, out rayHit, distance))
        {
            if (rayHit.collider.gameObject.GetComponent<Block>() == null)
            {
                arrowGuides[3].gameObject.SetActive(true);
                arrowGuides[5].gameObject.SetActive(true);
            }
        }
        else
        {
            arrowGuides[3].gameObject.SetActive(true);
            arrowGuides[5].gameObject.SetActive(true);
        }

        // x- 방향 체크
        ray.origin = origin.position;
        ray.direction = arrowGuides[1].position - arrowGuides[3].position;
        if (Physics.Raycast(ray.origin, ray.direction, out rayHit, distance))
        {
            if (rayHit.collider.gameObject.GetComponent<Block>() == null)
            {
                arrowGuides[1].gameObject.SetActive(true);
                arrowGuides[4].gameObject.SetActive(true);
            }
        }
        else
        {
            arrowGuides[1].gameObject.SetActive(true);
            arrowGuides[4].gameObject.SetActive(true);
        }

        // z+ 방향 체크
        ray.origin = origin.position;
        ray.direction = arrowGuides[2].position - arrowGuides[0].position;
        if (Physics.Raycast(ray.origin, ray.direction, out rayHit, distance))
        {
            if (rayHit.collider.gameObject.GetComponent<Block>() == null)
            {
                arrowGuides[2].gameObject.SetActive(true);
            }
        }
        else
        {
            arrowGuides[2].gameObject.SetActive(true);
        }

        // z- 방향 체크
        ray.origin = origin.position;
        ray.direction = arrowGuides[0].position - arrowGuides[2].position;
        if (Physics.Raycast(ray.origin, ray.direction, out rayHit, distance))
        {
            if (rayHit.collider.gameObject.GetComponent<Block>() == null)
            {
                arrowGuides[0].gameObject.SetActive(true);
            }
        }
        else
        {
            arrowGuides[0].gameObject.SetActive(true);
        }

        Block.IsUsingGuide = true;
        Block.GuidingBlockNum = BlockIdx;

        StartCoroutine(ArrowGuideTimerOn());
    }

    private IEnumerator ArrowGuideTimerOn()
    {
        var wait = new WaitForSeconds(2.0f);
        yield return wait;

        ArrowGuideOff();
    }

    public void ArrowGuideOff()
    {
        foreach (var arrow in arrowGuides)
        {
            arrow.gameObject.SetActive(false);
        }
        Block.IsUsingGuide = false;
    }
}