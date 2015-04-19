using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelParameters))]
public class LevelParametersEditor : Editor {

    public override void OnInspectorGUI()
    {
        LevelParameters myTarget = (LevelParameters)target;

        myTarget.testingMode = EditorGUILayout.Toggle("Turn on testing mode (fast movement)", myTarget.testingMode);
        myTarget.probabilityOfTilePaths = EditorGUILayout.Slider("Amount of tile paths", myTarget.probabilityOfTilePaths, 0, 100);
        myTarget.extraProbabilityOfJoinedTiles = EditorGUILayout.Slider("Extra probability of linked tiles", myTarget.extraProbabilityOfJoinedTiles, 0, 100);
        myTarget.guaranteedPathsFromFirstTile = EditorGUILayout.IntField("Guaranteed paths from first tile", myTarget.guaranteedPathsFromFirstTile);
        myTarget.numPathsBetweenOneFaceAndAnother = EditorGUILayout.IntSlider("Number of paths connecting faces", myTarget.numPathsBetweenOneFaceAndAnother, 0, 3);
        myTarget.startingCash = EditorGUILayout.IntField("Starting cash", myTarget.startingCash);
        myTarget.initialMineralYield = EditorGUILayout.IntField("Initial mineral amount on mineral tiles", myTarget.initialMineralYield);
        myTarget.mineralRefreshNumSeconds = EditorGUILayout.IntField("Seconds until mineral fields regen 1", myTarget.mineralRefreshNumSeconds);
        myTarget.percentageMineralsDrainedEachHarvest = EditorGUILayout.IntSlider("Percentage of minerals drained each harvest", myTarget.percentageMineralsDrainedEachHarvest, 0, 100);
        myTarget.oneCopyOfEachItem = EditorGUILayout.Toggle("One of each item up for grabs", myTarget.oneCopyOfEachItem);
        myTarget.offHandWeaponSpeedMultiplier = EditorGUILayout.FloatField("Offhand weapon speed multiplier", myTarget.offHandWeaponSpeedMultiplier);
    }
	
}
