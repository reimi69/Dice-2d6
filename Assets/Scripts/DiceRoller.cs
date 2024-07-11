using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceRoller : MonoBehaviour
{
    private GameObject dice1, dice2;
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Material material;
    [SerializeField] private Vector3 throwForce = new Vector3(0, 0, 10);
    private Rigidbody dice1Rigidbody;
    private Rigidbody dice2Rigidbody;
    private int resultDice1;
    private int resultDice2;
    [SerializeField] private float diceTimer = .5f;
    [SerializeField] private float rotationTime = 0.7f;
    private Vector3[] diceFaceRotations = {
        new Vector3(-90, 0, 0),       // 1
        new Vector3(0, 0, 0),      // 2
        new Vector3(0, 0, -90),      // 3
        new Vector3(0, 0, 90),     // 4
        new Vector3(180, 0, 0),     // 5
        new Vector3(90, 0, 0)      // 6
    };

    private float diceCooldown = 3f;
    private float diceCooldownTimer;

    [SerializeField] private float dissolveDuration = 2f; 
    void Start()
    {
        diceCooldownTimer = -diceCooldown;
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0) && diceCooldownTimer + diceCooldown <= Time.time)
        {
            RandomizeResult();
            StartCoroutine(ThrowDiceRoutine());
            diceCooldownTimer = Time.time;
        }
    }
    IEnumerator ThrowDiceRoutine()
    {
        DestroyIfNull();
        // Задаем начальные позиции кубиков
        dice1 = Instantiate(dicePrefab, spawnPoint1.position, Quaternion.identity);
        dice2 = Instantiate(dicePrefab, spawnPoint2.position, Quaternion.identity);
        // Кидаем
        dice1Rigidbody = dice1.GetComponent<Rigidbody>();
        dice1Rigidbody.AddForce(throwForce, ForceMode.Impulse);
        dice2Rigidbody = dice2.GetComponent<Rigidbody>();
        dice2Rigidbody.AddForce(throwForce, ForceMode.Impulse);
        // Ждем некоторое время, чтобы кубики бросились
        yield return new WaitForSeconds(diceTimer);

        // Останавливаем кубики на нужных значениях
        StartCoroutine(SmoothRotateToResult(dice1, resultDice1));
        StartCoroutine(SmoothRotateToResult(dice2, resultDice2));
    }
    IEnumerator SmoothRotateToResult(GameObject dice, int result)
    {
        Rigidbody rb = dice.GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
        Quaternion startRotation = dice.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(diceFaceRotations[result - 1]);

        float elapsedTime = 0f;
        while (elapsedTime < rotationTime)
        {
            dice.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        dice.transform.rotation = endRotation;

        material.SetFloat("_Cutoff", 0f);
        dice.GetComponent<MeshRenderer>().material = material;
        StartCoroutine(Dissolve());
    }
    // Плавное испарение
    IEnumerator Dissolve()
    {
        yield return new WaitForSeconds(1);
        float elapsedTime = 0f;
         
        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentCutoff = Mathf.Lerp(0f, 1f, elapsedTime / dissolveDuration);
            material.SetFloat("_Cutoff", currentCutoff);
            yield return null;
        }

        material.SetFloat("_Cutoff", 1f);
    }
    private void DestroyIfNull()
    {

        if (dice1 != null && dice2 != null)
        {
            Destroy(dice1);
            Destroy(dice2);
        }
        
    }
    private void RandomizeResult()
    {
        resultDice1 = Random.Range(1, 6);
        resultDice2 = Random.Range(1, 6);
        Debug.Log(resultDice1);
        Debug.Log(resultDice2);
    }

 
}
