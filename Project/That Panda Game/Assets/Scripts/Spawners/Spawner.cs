﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _object;

    [SerializeField]
    private float _minSpawnRate;
    [SerializeField]
    private float _maxSpawnRate;
    private float _spawnRate;

    [SerializeField]
    private float _spawnRadius;

    [SerializeField]
    private int _maxObjects;

    [SerializeField]
    private Vector3 _rotation;

    [SerializeField]
    private float _distanceFromApple;
    [SerializeField]
    private float _distanceFromCoin;
    [SerializeField]
    private float _distanceFromCookie;

    private GameObject _planet;

    private ObjectPool _objectPool;

    private List<GameObject> _activeObjects;
    public List<GameObject> ActiveObjects
    {
        get
        {
            return _activeObjects;
        }
        private set { }
    }

    private float _spawnTimer;
	
    void Awake()
    {
        SetSpawnRate();
        _planet = GameObject.Find("Planet");
        _activeObjects = new List<GameObject>();
        _objectPool = new ObjectPool(_object, _maxObjects);
    }

    public void Tick()
    {
        _spawnTimer += Time.deltaTime;
        if (_activeObjects.Count < _maxObjects && _spawnTimer > _spawnRate)
        {
            _spawnTimer = 0;
            SpawnObject();
        }
    }

    public GameObject SpawnObject(Vector3 pos)
    {
        GameObject obj = _objectPool.GetObject();
        _activeObjects.Add(obj);
        obj.transform.position = pos;
        obj.transform.LookAt(_planet.transform);
        obj.transform.Rotate(_rotation);
        obj.SetActive(true);
        return obj;
    }

    private void SpawnObject()
    {
        //Get an object from the object pool and position it on a random positon on a 2d plane above the planet
        Vector3 spawnPos = new Vector3(0, 0, -30);
        //Raycast towards the planet and get the Z position of the point it hits to place the object there
        RaycastHit hit;
        Physics.Raycast(spawnPos, Vector3.forward, out hit, 30);
        bool hitPlanet = false;

        int fallback = 0;

        List<GameObject> _coins = GameObject.Find("CoinSpawner").GetComponent<Spawner>().ActiveObjects;
        List<GameObject> _apples = GameObject.Find("AppleSpawner").GetComponent<Spawner>().ActiveObjects;
        List<GameObject> _cookies = GameObject.Find("CookieSpawner").GetComponent<Spawner>().ActiveObjects;


        //If the raycast doesn't hit the planet (hits an environment object, etc), try a new position
        while (!hitPlanet)
        {
            if (fallback > 50)
                return;
            spawnPos.x = Random.insideUnitCircle.x * _spawnRadius;
            spawnPos.y = Random.insideUnitCircle.y * _spawnRadius;

            if (CheckDistanceFromList(_coins, spawnPos, _distanceFromCoin))
            {
                fallback++;
                continue;
            }

            if (CheckDistanceFromList(_apples, spawnPos, _distanceFromApple))
            {
                fallback++;
                continue;
            }

            if (CheckDistanceFromList(_cookies, spawnPos, _distanceFromCookie))
            {
                fallback++;
                continue;
            }

            Physics.Raycast(spawnPos, Vector3.forward, out hit, 30);
            if (hit.collider.transform.name == "Planet")
                hitPlanet = true;
            fallback++;
        }
        GameObject obj = _objectPool.GetObject();
        spawnPos.z = hit.point.z - 1;
        _activeObjects.Add(obj);

        //Rotate the object to correctly allign it with the planet
        obj.transform.position = spawnPos;
        obj.transform.LookAt(_planet.transform);
        obj.transform.SetParent(_planet.transform, true);
        //If the object is a coin, add the GravitySim component to it
        Coin coin = obj.GetComponent<Coin>();
        if (coin != null)
        {
            coin.GetComponent<GravitySim>().enabled = true;
            coin.CanPickup();
        }
        obj.transform.Rotate(_rotation);
        obj.SetActive(true);
        SetSpawnRate();
    }

    private bool CheckDistanceFromList(List<GameObject> objects, Vector3 pos, float distance)
    {
        bool closeCoin = false;
        GameObject _closestCoin = null;

        if (objects.Count > 0)
        {
            _closestCoin = objects[0];
        }

        if (_closestCoin != null)
        {
            //Get the closest coin
            foreach (GameObject c in objects)
            {
                if ((new Vector2(c.transform.position.x, c.transform.position.y) - new Vector2(pos.x, pos.y)).magnitude < (new Vector2(_closestCoin.transform.position.x, _closestCoin.transform.position.y) - new Vector2(pos.x, pos.y)).magnitude)
                    _closestCoin = c;
            }
            //If the closest coin distance is less than the min distance allowed, try another position
            if ((new Vector2(_closestCoin.transform.position.x, _closestCoin.transform.position.y) - new Vector2(pos.x, pos.y)).magnitude < _distanceFromCoin)
            {
                closeCoin = true;
            }
            else
            {
                closeCoin = false;
            }
        }

        return closeCoin;
    }

    private void SetSpawnRate()
    {
        _spawnRate = Random.Range(_minSpawnRate, _maxSpawnRate);
    }


    public void Cleanup()
    {
        foreach (GameObject obj in _activeObjects)
            _objectPool.ReturnObject(obj);
        _activeObjects.Clear();
    }

    public void ReturnObject(GameObject obj)
    {
        _objectPool.ReturnObject(obj);
        _activeObjects.Remove(obj);
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
