using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    [Header("Block setting")]
    public Transform[] blockPrefabs;
    public int height = 17;
    public float zBias = 0.0f;
    public float spawnHeight = 1.0f;
    public float spacingBetweenBlocks = 1.0f;

    private List<Transform> blocks;
    private List<int[]> blockToBeDeleted;
    public bool bGameRunning = false;
    public bool bDestroying = false;
    private bool bRotating = false;

    [Header("UI setting")]
    public GameObject gameOverUI;
    public GameObject gameStartmenuUI;
    public GameObject gamePlayingUI;

    [Header("Game setting")]
    public GameObject plane;
    private int gameScore = 0;
    private GameObject device;
    private int nearestBlockIdx = -1;

    [Header("Audio")]
    public AudioClip audioDestroyBlock;
    public AudioClip audioGameOver;
    public AudioClip audioGameStart;
    private AudioSource audioSource;

    private int[] getRandomBlockType()
    {
        int[] blockTypes = new int[3];
        blockTypes[0] = Random.Range(0, blockPrefabs.Length);
        blockTypes[1] = Random.Range(0, blockPrefabs.Length);
        blockTypes[2] = Random.Range(0, blockPrefabs.Length);
        return blockTypes;
    }

    private int[] getRandomBlockTypeWithoutOverlap()
    {
        int[] blockTypes = new int[3] { 0, 1, 2 };
        for (int i = 0; i < 2; i++)
        {
            int RandomIdx = Random.Range(i, 3);
            int temp = blockTypes[i];
            blockTypes[i] = blockTypes[RandomIdx];
            blockTypes[RandomIdx] = temp;
        }
        return blockTypes;
    }

    private void Start()
    {
        gameOverUI.SetActive(false);
        gameStartmenuUI.SetActive(true);
        gamePlayingUI.SetActive(false);
        device = GameObject.Find("Device");
        plane = GameObject.Find("Plane");
        audioSource = GetComponent<AudioSource>();
        blockToBeDeleted = new List<int[]>();
    }

    private void Update()
    {
        if (bGameRunning)
        {
            // Find the block closest to the device
            //if (device != null) nearestBlockIdx = FindNearBlockNum(device.transform.position);

            if (!bDestroying && blockToBeDeleted.Count == 0)
            {
                // Check game over
                CheckGameOver();
                // Check if blocks of the same color are on one line
                CheckBlockInLine();
            }
            else if (!bDestroying && blockToBeDeleted.Count != 0)
            {
                //StartCoroutine(DestroyBlocksCorutine());
                DestoryBlocks();
            }
        }
    }

    private IEnumerator GameSet()
    {
        // init score
        gameScore = 0;
        // init numOfCollided in Plane
        if (plane.GetComponent<CheckGameOver>() != null)
        {
            plane.GetComponent<CheckGameOver>().ResetNumOfObjectCollided();
        }
        // init blocktobedeleted
        blockToBeDeleted.Clear();
        bDestroying = true;
        // Set blocks for game
        blocks = new List<Transform>(new Transform[height * 3]);

        var wait = new WaitForSeconds(0.1f);

        for (int i = 0; i < height; ++i)
        {
            if (i % 2 == 0)
            {
                BuildVerticalLayer(i);
            }
            else
            {
                BuildHorizontalLayer(i);
            }
            yield return wait;
        }
        StartCoroutine(Stabilize());
    }
    public void GameStart()
    {
        StartCoroutine(GameSet());
        gameStartmenuUI.SetActive(false);
        gamePlayingUI.SetActive(true);
    }

    public void GameReset()
    {
        // Game reset
        bGameRunning = false;
        // destroy current existing blocks
        foreach (var block in blocks)
        {
            Destroy(block.gameObject);
        }
        blocks.Clear();
        // game over ui off
        gameOverUI.SetActive(false);
        // game playing ui on
        gamePlayingUI.SetActive(true);
        GameObject.Find("SerialReader").GetComponent<GyroV2>().jenga = null;
        // block setting
        StartCoroutine(GameSet());
    }

    private void CheckGameOver()
    {
        int numOfFallingBlocks = 0;
        foreach (var block in blocks)
        {
            if (block == null) continue;
            Rigidbody body = block.GetComponent<Rigidbody>();
            if (body.velocity.y < -4.0f)
            {
                numOfFallingBlocks += 1;
            }
        }
        // get num of collided object
        bool IsGameOver = false;
        if (plane.GetComponent<CheckGameOver>() != null)
        {
            IsGameOver = plane.GetComponent<CheckGameOver>().IsGameOver();
        }
        // if the number of falling blocks is more than 5
        //if (numOfFallingBlocks > 10 || IsGameOver)
        if (IsGameOver)
        {
            // GameOver UI Active
            gameOverUI.SetActive(true);
            gamePlayingUI.SetActive(false);
            bGameRunning = false;
            // audio 
            audioSource.clip = audioGameOver;
            audioSource.Play();
        }
    }

    private void CheckBlockInLine()
    {
        float refHeight = 0; //기준 높이
        int refBlockType = -1; //이전에 찾은 block의 index
        int iTempBlock = 0; //tempBlock의 block index
        int blockIndex = -1; //block의 index
        bool bDestroy = true;

        for (int i = 0; i < height; ++i)
        {
            int[] tempBlockIndex = new int[3];
            refHeight = Block.height / 2.0f + i * Block.height;
            iTempBlock = 0;
            blockIndex = -1;
            bDestroy = true;

            // Find Block and block's color type
            foreach (var block in blocks)
            {
                if (iTempBlock > 2)
                {
                    break;
                }
                if(block.gameObject.GetComponent<Block>().IsSelected)
                {
                    continue;
                }
                blockIndex += 1;
                //if(block.transform.position.x < -2.0f || block.transform.position.x > 2.0f || block.transform.position.z < -2.0f)
                //find block located at that height and not falling
                //refheight - 0.1f < BlockHeight < refHeight + 0.1f
                if (refHeight - 0.3f < block.transform.position.y && block.transform.position.y < refHeight + 0.3f)
                {
                    if(block.transform.position.x < -3.0f || block.transform.position.x > 3.0f || block.transform.position.z < -3.0f || block.transform.position.z > 3.0f)
                    {
                        continue;
                    }
                    if (!IsStableBlock(block))
                    {
                        continue;
                    }
                    if (iTempBlock == 0)
                    {
                        // 해당 height에서 처음 찾은 블럭
                        tempBlockIndex[iTempBlock] = blockIndex;
                        refBlockType = block.GetComponent<Block>().BlockType;
                    }
                    else
                    {
                        // 해당 height에서 찾은 다음 블럭
                        if (refBlockType == block.GetComponent<Block>().BlockType)
                        {
                            tempBlockIndex[iTempBlock] = blockIndex;
                        }
                        else
                        {
                            bDestroy = false;
                            break;
                        }
                    }
                    iTempBlock += 1;
                }
            }

            // destroy 판정
            if (bDestroy && iTempBlock >= 3)
            {
                blockToBeDeleted.Add(tempBlockIndex);
            }
        }

        //Destroy할 블럭이 존재 -> Destroy
        /*
        if (blockDestroyIndex.Count != 0)
        {
            bGameRunning = false;
            DestoryBlocks(blockDestroyIndex);
        }
        */
    }

    private bool IsStableBlock(Transform refBlock)
    {
        Rigidbody body = refBlock.GetComponent<Rigidbody>();
        if (body.velocity.magnitude > 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DestroyBlockInLine(int[] refIndex)
    {
        //내림차순 정렬
        int temp;
        for (int i = 0; i < refIndex.Length; i++)
        {
            for (int j = i; j < refIndex.Length; j++)
            {
                if (refIndex[i] < refIndex[j])
                {
                    temp = refIndex[j];
                    refIndex[j] = refIndex[i];
                    refIndex[i] = temp;
                }
            }
        }
        int blockLargestIndex = refIndex[0];
        //destroy block
        foreach (var index in refIndex)
        {
            Destroy(blocks[index].gameObject);
            blocks.RemoveAt(index);
        }
        //rotate
        for (int i = blockLargestIndex - 2; i < blocks.Count; i++)
        {
            // rotate Blocks
            if (blocks[i].transform.rotation.eulerAngles.y > 80.0f && blocks[i].transform.rotation.eulerAngles.y < 100.0f)
            {
                Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                blocks[i].transform.rotation = rotation;
                blocks[i].transform.position = new Vector3(
                    blocks[i].transform.position.z,
                    blocks[i].transform.position.y,
                    blocks[i].transform.position.x
                );
            }
            else
            {
                Quaternion rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                blocks[i].transform.rotation = rotation;
                blocks[i].transform.position = new Vector3(
                    blocks[i].transform.position.z,
                    blocks[i].transform.position.y,
                    blocks[i].transform.position.x
                );
            }
            blocks[i].GetComponent<Block>().BlockIdx -= 3;
        }
    }

    private void DestoryBlocks()
    {
        bDestroying = true;
        blockToBeDeleted.Reverse();
        foreach (int[] blockIndexArr in blockToBeDeleted)
        {
            DestroyBlockInLine(blockIndexArr);
            updateScore(100);
        }
        blockToBeDeleted.Clear();
        bDestroying = false;
        audioSource.clip = audioDestroyBlock;
        audioSource.Play();
        //StartCoroutine(Stabilize());
        if (bDestroying) bDestroying = false;
        bGameRunning = true;
    }

    private IEnumerator DestroyBlocksCorutine()
    {
        var wait = new WaitForSeconds(1.0f);

        bDestroying = true;
        blockToBeDeleted.Reverse();

        int index = 0;
        foreach (int[] blockIndexArr in blockToBeDeleted)
        {
            for (int i = 0; i < blockIndexArr.Length; i++)
            {
                blockIndexArr[i] += index * 3;
            }
            DestroyBlockInLine(blockIndexArr);
            updateScore(100);
            yield return wait;
            AddBlockOneLine();
            yield return wait;
            index += 1;
        }

        blockToBeDeleted.Clear();
        //bDestroying = false;
        StartCoroutine(Stabilize());
    }

    private IEnumerator Stabilize()
    {
        var wait = new WaitForSeconds(0.05f);

        foreach (Transform block in blocks)
        {
            if (block == null) continue;
            Rigidbody body = block.GetComponent<Rigidbody>();
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            yield return wait;
        }
        if (bDestroying) bDestroying = false;
        bGameRunning = true;
    }

    private void AddBlockOneLine()
    {
        //add blocks
        float y = spawnHeight;
        int[] blockTypes = getRandomBlockTypeWithoutOverlap();

        Transform block = Instantiate(blockPrefabs[blockTypes[0]], transform);
        block.localPosition = new Vector3(0f, y, Block.length / 2.0f);
        block.GetComponent<Block>().BlockType = blockTypes[0];
        block.GetComponent<Block>().BlockIdx = 0;
        blocks.Insert(0, block);

        block = Instantiate(blockPrefabs[blockTypes[1]], transform);
        block.localPosition = new Vector3(Block.width + Block.deformation * 2, y, Block.length / 2.0f);
        block.GetComponent<Block>().BlockType = blockTypes[1];
        block.GetComponent<Block>().BlockIdx = 1;
        blocks.Insert(1, block);

        block = Instantiate(blockPrefabs[blockTypes[2]], transform);
        block.localPosition = new Vector3(-Block.width - Block.deformation * 2, y, Block.length / 2.0f);
        block.GetComponent<Block>().BlockType = blockTypes[2];
        block.GetComponent<Block>().BlockIdx = 2;
        blocks.Insert(2, block);

        // height 1 증가
        height++;

        // 기존 블럭의 위치 조정
        for (int i = blocks.Count - 1; i > 2; i--)
        {
            //위치 조정
            blocks[i].transform.position = new Vector3(
                    blocks[i].transform.position.x,
                    blocks[i].transform.position.y + Block.height + Block.deformation,
                    blocks[i].transform.position.z
                );
            //로테이션
            if (blocks[i].transform.rotation.eulerAngles.y > 80.0f && blocks[i].transform.rotation.eulerAngles.y < 100.0f)
            {
                Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                blocks[i].transform.rotation = rotation;
                blocks[i].transform.position = new Vector3(
                    blocks[i].transform.position.z,
                    blocks[i].transform.position.y,
                    blocks[i].transform.position.x
                );
            }
            else
            {
                Quaternion rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                blocks[i].transform.rotation = rotation;
                blocks[i].transform.position = new Vector3(
                    blocks[i].transform.position.z,
                    blocks[i].transform.position.y,
                    blocks[i].transform.position.x
                );
            }
            //인덱스 조정
            blocks[i].GetComponent<Block>().BlockIdx += 3;
        }
    }

    private void BuildVerticalLayer(int layerIndex)
    {
        float y = (Block.height + Block.deformation + spacingBetweenBlocks) * layerIndex + spawnHeight;

        int[] blockTypes = getRandomBlockType();

        Transform block = Instantiate(blockPrefabs[blockTypes[0]], transform);
        block.localPosition = new Vector3(0f, y, Block.length / 2.0f);
        block.GetComponent<Block>().BlockType = blockTypes[0];
        block.GetComponent<Block>().BlockIdx = 3 * layerIndex + 0;
        blocks[3 * layerIndex + 0] = block;

        block = Instantiate(blockPrefabs[blockTypes[1]], transform);
        block.localPosition = new Vector3(Block.width + Block.deformation * 2, y, Block.length / 2.0f);
        block.GetComponent<Block>().BlockType = blockTypes[1];
        block.GetComponent<Block>().BlockIdx = 3 * layerIndex + 1;
        blocks[3 * layerIndex + 1] = block;

        block = Instantiate(blockPrefabs[blockTypes[2]], transform);
        block.localPosition = new Vector3(-Block.width - Block.deformation * 2, y, Block.length / 2.0f);
        block.GetComponent<Block>().BlockType = blockTypes[2];
        block.GetComponent<Block>().BlockIdx = 3 * layerIndex + 2;
        blocks[3 * layerIndex + 2] = block;
    }

    private void BuildHorizontalLayer(int layerIndex)
    {
        Quaternion rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        float y = (Block.height + Block.deformation + spacingBetweenBlocks) * layerIndex + spawnHeight;

        int[] blockTypes = getRandomBlockType();

        Transform block = Instantiate(blockPrefabs[blockTypes[0]], transform);
        block.localRotation = rotation;
        block.localPosition = new Vector3(Block.length / 2.0f, y, 0.0f);
        block.GetComponent<Block>().BlockType = blockTypes[0];
        block.GetComponent<Block>().BlockIdx = 3 * layerIndex + 0;
        blocks[3 * layerIndex + 0] = block;

        block = Instantiate(blockPrefabs[blockTypes[1]], transform);
        block.localRotation = rotation;
        block.localPosition = new Vector3(Block.length / 2.0f, y, Block.width + Block.deformation * 2);
        block.GetComponent<Block>().BlockType = blockTypes[1];
        block.GetComponent<Block>().BlockIdx = 3 * layerIndex + 1;
        blocks[3 * layerIndex + 1] = block;

        block = Instantiate(blockPrefabs[blockTypes[2]], transform);
        block.localRotation = rotation;
        block.localPosition = new Vector3(Block.length / 2.0f, y, -Block.width - Block.deformation * 2);
        block.GetComponent<Block>().BlockType = blockTypes[2];
        block.GetComponent<Block>().BlockIdx = 3 * layerIndex + 2;
        blocks[3 * layerIndex + 2] = block;
    }

    private void updateScore(int updateScore)
    {
        gameScore += updateScore;
    }

    public int getScore()
    {
        return gameScore;
    }

    public int getNearestBlockIdx()
    {
        return nearestBlockIdx;
    }

    public int FindNearBlockNum(Vector3 refPos)
    {
        int num = -1;
        int i = 0;
        float shortestDistance = 5.0f;
        foreach (var block in blocks)
        {
            float dis = Vector3.Distance(refPos, block.position);
            if (dis < shortestDistance)
            {
                shortestDistance = dis;
                num = i;
            }
            i++;
        }
        return num;
    }
}