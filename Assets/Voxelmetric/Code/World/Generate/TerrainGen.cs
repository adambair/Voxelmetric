﻿using UnityEngine;
using System.Collections;
using SimplexNoise;

public class TerrainGen
{

    int stoneBaseHeight = -20;
    float stoneBaseNoise = 0.03f;
    int stoneBaseNoiseHeight = 10;

    int stoneMountainHeight = 10;
    float stoneMountainFrequency = 0.008f;
    int stoneMinHeight = 0;

    int dirtBaseHeight = 1;
    float dirtNoise = 0.04f;
    int dirtNoiseHeight = 2;

    public void ChunkGen(Chunk chunk)
    {


        for (int x = 0; x < Config.Env.ChunkSize; x++)
        {
            for (int z = 0; z < Config.Env.ChunkSize; z++)
            {
                GenerateTerrain(chunk, x, z);
            }
        }

        for (int x = -3; x < Config.Env.ChunkSize +3; x++)
        {
            for (int z = -3; z < Config.Env.ChunkSize +3; z++)
            {
                CreateTreeIfValid(x, z, chunk);
            }
        }

    }

    int LayerStoneBase(int x, int z)
    {
        int stoneHeight = stoneBaseHeight;
        stoneHeight += GetNoise(x, 0, z, stoneMountainFrequency, stoneMountainHeight, 1.6f);
        stoneHeight += GetNoise(x, 1000, z, 0.03f, 8, 1) * 2;

        if (stoneHeight < stoneMinHeight)
            return stoneMinHeight;

        return stoneHeight; 
    }

    int LayerStoneNoise(int x, int z)
    {
        return GetNoise(x, 0, z, stoneBaseNoise, stoneBaseNoiseHeight, 1);
    }

    int LayerDirt(int x, int z)
    {
        int dirtHeight = dirtBaseHeight;
        dirtHeight += GetNoise(x, 100, z, dirtNoise, dirtNoiseHeight, 1);
       
        return dirtHeight;
    }

    void GenerateTerrain(Chunk chunk, int x, int z)
    {
        int stoneHeight = LayerStoneBase(chunk.pos.x + x, chunk.pos.z + z);
        stoneHeight += LayerStoneNoise(chunk.pos.x + x, chunk.pos.z + z);

        int dirtHeight = stoneHeight + LayerDirt(chunk.pos.x + x, chunk.pos.z + z);
        //CreateTreeIfValid(x, z, chunk, dirtHeight);

        for (int y = 0; y < Config.Env.ChunkSize; y++)
        {

            if (y + chunk.pos.y <= stoneHeight)
            {
                SetBlock(chunk, Block.Stone, new BlockPos(x,y,z));
            }
            else if (y + chunk.pos.y < dirtHeight)
            {
                SetBlock(chunk, Block.Dirt, new BlockPos(x,y,z));
            }
            else if (y + chunk.pos.y == dirtHeight)
            {
                SetBlock(chunk, Block.Grass, new BlockPos(x,y,z));
            }

        }

    }

    public static void SetBlock(Chunk chunk, Block block, BlockPos pos, bool replaceBlocks = false)
    {
        if (Chunk.InRange(pos))
        {
            if (replaceBlocks || chunk.GetBlock(pos).type == Block.Air.type)
            {
                block.modified = false;
                chunk.SetBlock(pos, block, false);
            }
        }
    }

    public static int GetNoise(int x, int y, int z, float scale, int max, float power)
    {
        float noise = (Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f);

        noise = Mathf.Pow(noise, power);

        return Mathf.FloorToInt(noise);
    }

    void CreateTreeIfValid(int x, int z, Chunk chunk)
    {
        if (GetNoise(x + chunk.pos.x, 0, z + chunk.pos.z, 0.2f, 100, 1) < 3)
        {
            int terrainHeight = LayerStoneBase(x + chunk.pos.x, z + chunk.pos.z);
            terrainHeight += LayerStoneNoise(x + chunk.pos.x, z + chunk.pos.z);
            terrainHeight += LayerDirt(x + chunk.pos.x, z + chunk.pos.z); ;


            if (StructureTree.ChunkContains(chunk, new BlockPos(x + chunk.pos.x, terrainHeight, z + chunk.pos.z)))
            {
                StructureTree.Build(chunk, new BlockPos(x, terrainHeight - chunk.pos.y, z));
            }
        }
    }

}