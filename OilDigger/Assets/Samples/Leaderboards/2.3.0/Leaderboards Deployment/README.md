# Leaderboards Deployment

This sample shows a simple way to deploy a Leaderboard configuration, as well as add and get a player score from the runtime API.

## Using the Sample

### Deploying your Leaderboard configuration

In order to deploy your configuration to the Leaderboard service, do the following:

1. Link your unity project in `Project Settings > Services`.
2. Select your desired environment in `Project Settings > Services > Environments`.
3. Deploy your [Leaderboard Configuration](./Sample_Leaderboard.lb) in the [Deployment window](https://docs.unity3d.com/Packages/com.unity.services.deployment@latest/manual/deployment_window.html).

### Play the Scene

To test out the sample, open the `LeaderboardsDeployment` scene in the Editor and click `Play`.

You can add a score to the leaderboard by typing a number into the input field and clicking the `Add score` button.
A message will appear in the console to inform you that the score has been recorded.

You can click the `Log score` button to log the current top score to the console.

You can modify the Leaderboard configuration and deploy it while the scene is playing. 
Try changing the `SortOrder` from `asc` to `desc` to invert whether a high or low score is best. 

## Package Dependencies

This sample has dependencies to other packages.
If your project does not have these packages, they will be installed.
If your project has the packages installed but they are of a previous version, they will be updated.
A log message indicating which package is installed at which version will be displayed in the console.

The following packages are required for this sample:
- `com.unity.services.authentication@3.2.0`
- `com.unity.services.leaderboards@2.0.0`

### Unity UI / Text Mesh Pro

This sample uses Unity UI and Text Mesh Pro. In 2022 and below, this will install the `com.unity.textmeshpro` package and prompt you to install the TMP Essential Assets.

In Unity 6 and above, Text Mesh Pro has been integrated to the `com.unity.ugui` package. On this version, the TMP Essential Assets will automatically be installed.

## Troubleshooting

### Empty Deployment window

If the deployment window is empty, make sure your project is linked in `Project Settings > Services` and that you have selected an environment in `Project Settings > Services > Environments`.
