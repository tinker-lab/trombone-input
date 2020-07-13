﻿using System.Collections;
using UnityEngine;

namespace Utils.Floor
{
    class Tile
    {
        public GameObject theTile;
        public float creationTime;

        public Tile(GameObject t, float ct)
        {
            theTile = t;
            creationTime = ct;
        }
    }

    public class GenerateInfinite : MonoBehaviour
    {
        public GameObject plane;
        public GameObject player;

        int planeSize = 10;
        int halfTilesX = 5;
        int halfTilesZ = 5;

        Vector3 startPos;

        Hashtable tiles = new Hashtable();

        // Start is called before the first frame update
        void Start()
        {
            this.gameObject.transform.position = Vector3.zero;
            startPos = Vector3.zero;

            float updateTime = Time.realtimeSinceStartup;

            for (int x = -halfTilesX; x < halfTilesX; x++)
            {
                for (int z = -halfTilesZ; z < halfTilesZ; z++)
                {
                    Vector3 pos = new Vector3((x * planeSize + startPos.x),
                                                0,
                                                (z * planeSize + startPos.z));
                    GameObject t = Instantiate(plane, pos, Quaternion.identity, transform);

                    string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();

                    t.name = tilename;
                    Tile tile = new Tile(t, updateTime);
                    tiles.Add(tilename, tile);
                }
            }

        }

        // Update is called once per frame
        void Update()
        {
            int xMove = (int)(player.transform.position.x - startPos.x);
            int zMove = (int)(player.transform.position.z - startPos.z);

            if (Mathf.Abs(xMove) >= planeSize || Mathf.Abs(zMove) >= planeSize)
            {
                float updateTime = Time.realtimeSinceStartup;

                //force integer position and round to nearest tilesSize
                int playerX = (int)(Mathf.Floor(player.transform.position.x / planeSize) * planeSize);
                int playerZ = (int)(Mathf.Floor(player.transform.position.z / planeSize) * planeSize);

                for (int x = -halfTilesX; x < halfTilesX; x++)
                {
                    for (int z = -halfTilesZ; z < halfTilesZ; z++)
                    {
                        Vector3 pos = new Vector3((x * planeSize + playerX), 0, (z * planeSize + playerZ));
                        string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();

                        if (!tiles.ContainsKey(tilename))
                        {
                            GameObject t = Instantiate(plane, pos, Quaternion.identity, transform);
                            t.name = tilename;
                            Tile tile = new Tile(t, updateTime);
                            tiles.Add(tilename, tile);
                        }
                        else
                        {
                            (tiles[tilename] as Tile).creationTime = updateTime;
                        }
                    }
                }

                //destroy all tiles not just created or with thime upate
                //and put new tiles and tiles to be kepts in a new Hashtable
                Hashtable newTerrain = new Hashtable();
                foreach (Tile tls in tiles.Values)
                {
                    if (tls.creationTime != updateTime)
                    {
                        //delete gameobjct
                        Destroy(tls.theTile);
                    }
                    else
                    {
                        newTerrain.Add(tls.theTile.name, tls);
                    }
                }
                //coppy new hashtable contents to the working hastbale
                tiles = newTerrain;

                startPos = player.transform.position;
            }

        }
    }
}