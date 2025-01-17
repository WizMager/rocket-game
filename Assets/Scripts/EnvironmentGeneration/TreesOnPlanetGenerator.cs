﻿using System;
using System.Collections.Generic;
using ScriptableData;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace EnvironmentGeneration
{
    public class TreesOnPlanetGenerator
    {
        private readonly List<GameObject> _treesPrefabs;
        private readonly float _planetRadius;
        private readonly GameObject _rootTreesOnPlanet;
        private readonly List<Transform> _spawnedTopTrees;
        private readonly List<Transform> _spawnedDownTrees;
        private readonly int _treesOnPlanet;

        public TreesOnPlanetGenerator(AllData data, float planetRadius, GameObject rootEnvironment)
        {
            _treesPrefabs = new List<GameObject>(data.Prefab.trees);
            var treesMaterials = new List<Material[]>(GetTreesMaterials(data.Prefab.trees.Length, data.Materials));
            for (int i = 0; i < data.Prefab.trees.Length; i++)
            {
                _treesPrefabs.Add(PaintTree(data.Prefab.trees[i], treesMaterials[i]));
            }
            _spawnedTopTrees = new List<Transform>();
            _spawnedDownTrees = new List<Transform>();
            

            _planetRadius = planetRadius;
            _rootTreesOnPlanet = new GameObject("TreesOnPlanet");
            _rootTreesOnPlanet.transform.SetParent(rootEnvironment.transform);
            _treesOnPlanet = data.ObjectsOnPlanetData.treesOnPlanet;
        }

        private List<Material[]> GetTreesMaterials(int treePrefabs, MaterialsData materialsData)
        {
            var treesMaterials = new List<Material[]>
            {
                materialsData.tree1Type,
                materialsData.tree2Type,
                materialsData.tree3Type,
                materialsData.tree4Type
            };
            if (treesMaterials.Count != treePrefabs)
            {
                throw new ArgumentOutOfRangeException(
                    "Number of tree prefabs is does not mach with number materials types");
            }

            return treesMaterials;
        }
        
        private GameObject PaintTree(GameObject tree, Material[] treeMaterials)
        {
            var meshRenderers = tree.GetComponentsInChildren<MeshRenderer>();
            var randomMaterialNumber = Random.Range(0, treeMaterials.Length / 2 - 1);
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer.gameObject.CompareTag("Crown"))
                {
                    meshRenderer.material = treeMaterials[randomMaterialNumber];
                }

                if (meshRenderer.gameObject.CompareTag("Trunk"))
                {
                    meshRenderer.material = treeMaterials[randomMaterialNumber + treeMaterials.Length / 2];
                }
            }

            return tree;
        }

        public List<Transform> CreateTopTreesAndPosition(List<PlanetCell> planetCellsDown)
        {
            var createdTrees = 0;
            var halfTreesOnPlanet = Mathf.RoundToInt(_treesOnPlanet / 2);
            do
            {
                var randomCell = Random.Range(0, planetCellsDown.Count);
                if (planetCellsDown[randomCell].IsOccupied) continue;
                var tempCell = planetCellsDown[randomCell];
                tempCell.Occupied();
                planetCellsDown[randomCell] = tempCell;
                createdTrees++;
                var randomTreeType = Random.Range(0, _treesPrefabs.Count);
                var positionAndRotation = GeneratePositionAndRotation(planetCellsDown[randomCell]);
                var tree = Object.Instantiate(_treesPrefabs[randomTreeType], positionAndRotation.Item1, positionAndRotation.Item2);
                _spawnedTopTrees.Add(tree.transform);
                //TODO: here can realize rotate around itself
                //tree.transform.RotateAround(tree.transform.position, tree.transform.up, randomAngleRotationBuilding);
                tree.transform.SetParent(_rootTreesOnPlanet.transform);
            } while (halfTreesOnPlanet > createdTrees);

            return _spawnedTopTrees;
        }

        public List<Transform> CreateDownTreesAndPosition(List<PlanetCell> planetCellsDown)
        {
            var createdTrees = 0;
            var halfTreesOnPlanet = Mathf.RoundToInt(_treesOnPlanet / 2);
            do
            {
                var randomCell = Random.Range(0, planetCellsDown.Count);
                if (planetCellsDown[randomCell].IsOccupied) continue;
                var tempCell = planetCellsDown[randomCell];
                tempCell.Occupied();
                planetCellsDown[randomCell] = tempCell;
                createdTrees++;
                var randomTreeType = Random.Range(0, _treesPrefabs.Count);
                var positionAndRotation = GeneratePositionAndRotation(planetCellsDown[randomCell]);
                var tree = Object.Instantiate(_treesPrefabs[randomTreeType], positionAndRotation.Item1, positionAndRotation.Item2);
                _spawnedDownTrees.Add(tree.transform);
                tree.transform.RotateAround(tree.transform.position, tree.transform.forward, 180);
                tree.transform.SetParent(_rootTreesOnPlanet.transform);
            } while (halfTreesOnPlanet > createdTrees);

            return _spawnedDownTrees;
        }

        private (Vector3, Quaternion) GeneratePositionAndRotation(PlanetCell planetCell)
        {
            var randomX = Random.Range(planetCell.rangeX.x, planetCell.rangeX.y);
            var vectorUp = Vector3.up;
            if (randomX > 90f)
            {
                vectorUp = -Vector3.up;
            }
            var randomY = Random.Range(planetCell.rangeY.x, planetCell.rangeY.y);
            var randomZ = Random.Range(planetCell.rangeZ.x, planetCell.rangeZ.y);
            var rotation = Quaternion.Euler(randomX, randomY, randomZ);
            var position = rotation * vectorUp * _planetRadius;
            return (position, rotation);
        }
    }
}