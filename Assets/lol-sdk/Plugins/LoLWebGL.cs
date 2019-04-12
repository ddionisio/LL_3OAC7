using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;
using SimpleJSON;
/**
	Version 2.0.0 RC-10
*/
namespace LoLSDK
{
	public class WebGL : ILOLSDK
	{
		// *************************************
		// PLUMBING
		// *************************************

		[DllImport("__Internal")]
		public static extern void _PostWindowMessage (string msgName, string jsonPayload);
		public void PostWindowMessage (string msgName, string jsonPayload) {
			Debug.Log("PostWindowMessage " + msgName);

			_PostWindowMessage(msgName, jsonPayload);
		}

		public void LogMessage (string msg) {
			JSONNode payload = JSON.Parse("{}");

			payload["msg"] = msg;

			PostWindowMessage("logMessage", payload.ToString());
		}

		// *************************************
		// GAME LIFECYCLE
		// *************************************

		[DllImport("__Internal")]
		private static extern string _GameIsReady (string gameName, string callbackGameObject, string aspectRatio, string resolution, string sdkVersion);
		public void GameIsReady (string gameName, string callbackGameObject, string aspectRatio, string resolution) {
			_GameIsReady(gameName, callbackGameObject, aspectRatio, resolution, "V2");
		}

		public void CompleteGame () {
			JSONNode payload = JSON.Parse("{}");
			PostWindowMessage("complete", payload.ToString());
		}

		// *************************************
		// ANSWER SUBMISSION
		// *************************************

		public void SubmitProgress (int score, int currentProgress, int maximumProgress = -1) {
			JSONNode payload = JSON.Parse("{}");

			payload["score"].AsInt = score;
			payload["currentProgress"].AsInt = currentProgress;
			payload["maximumProgress"].AsInt = maximumProgress;

			PostWindowMessage("progress", payload.ToString());
		}

		public void SubmitAnswer (int questionId, int alternativeId) {
			JSONNode payload = JSON.Parse("{}");

			payload["questionId"].AsInt = questionId;
			payload["alternativeId"].AsInt = alternativeId;

			PostWindowMessage("answer", payload.ToString());
		}

		// *************************************
		// SPEECH
		// *************************************

		public void SpeakText (string key) {
			JSONNode  payload = JSON.Parse("{}");

			payload["key"] = key;

			PostWindowMessage("speakText", payload.ToString());
		}

		public void SpeakQuestion (int questionId) {
			Debug.Log("SpeakQuestion");

			JSONNode payload = JSON.Parse("{}");

			payload["questionId"].AsInt = questionId;

			PostWindowMessage("speakQuestion", payload.ToString());
		}

		public void SpeakAlternative (int alternativeId) {
			JSONNode payload = JSON.Parse("{}");

			payload["alternativeId"].AsInt = alternativeId;

			PostWindowMessage("speakAlternative", payload.ToString());
		}

		public void SpeakQuestionAndAlternatives (int questionId) {
			JSONNode payload = JSON.Parse("{}");

			payload["questionId"].AsInt = questionId;

			PostWindowMessage("speakQuestionAndAlternatives", payload.ToString());
		}

		public void Error (string msg) {
			JSONNode payload = JSON.Parse("{}");

			payload["msg"] = msg;

			PostWindowMessage("error", payload.ToString());
		}

		// *************************************
		// PLAY, STOP, and CONFIGURE SOUNDS
		// *************************************

		public void PlaySound (string file, bool background = false, bool loop = false) {
			JSONNode payload = JSON.Parse("{}");

			payload["file"] = file;
			payload["background"] = background;
			payload["loop"] = loop;

			PostWindowMessage("playSound", payload.ToString());
		}

		public void ConfigureSound (float foreground, float background, float fade) {
			JSONNode payload = JSON.Parse("{}");

			payload["foreground"].AsFloat = foreground;
			payload["background"].AsFloat = background;
			payload["fade"].AsFloat = fade;

			PostWindowMessage("configureSound", payload.ToString());
		}

		public void StopSound (string file) {
			JSONNode payload = JSON.Parse("{}");

			payload["file"] = file;

			PostWindowMessage("stopSound", payload.ToString());
		}
	}

	public class MockWebGL : ILOLSDK
	{

		/* *********************************************
 		 * MOCK Messages for Local Development
		 * *********************************************

		/* *********************************************
 		 * PLUMBING
		 ********************************************** */

		public void PostWindowMessage (string msgName, string jsonPayload) {
			Debug.Log ("PostWindowMessage: " + msgName);
			Debug.Log ("JSON: " + jsonPayload);
		}

		public void LogMessage (string msg) {
			Debug.Log ("LogMessage");
		}
		/* *********************************************
 		 * ERROR
		 ********************************************** */

		public void sError (string msg) {
			Debug.Log ("Error");
		}

		/* *********************************************
 		 * GAME LIFECYCLE
		 ********************************************** */

		public void CompleteGame () {
			Debug.Log ("CompleteGame");
		}

		public void GameIsReady (string gameName, string callbackGameObject, string aspectRatio, string resolution)
		{
			Debug.Log ("GameIsReady Editor");
			Debug.Log ("GameIsReady gameName" + gameName);
			Debug.Log ("GameIsReady callbackGameObject" + callbackGameObject);
		}

		/* *********************************************
 		 * SUBMIT PROGRESS AND ANSWERS
		 ********************************************** */

		public void SubmitProgress (int score, int currentProgress, int maximumProgress = -1) {
			Debug.Log ("SubmitProgress");
		}

		public void SubmitAnswer (int questionId, int alternativeId) {
			Debug.Log ("SubmitAnswer");
		}

		/* *********************************************
 		 * SPEECH
		 ********************************************** */

		public void SpeakText (string key) {
			Debug.Log ("SpeakText");
		}

		public void SpeakQuestion (int questionId) {
			Debug.Log ("SpeakQuestion");
		}

		public void SpeakQuestionAndAlternatives (int questionId) {
			Debug.Log ("SpeakQuestionAndAlternatives");
		}

		public void SpeakAlternative (int alternativeId) {
			Debug.Log ("SpeakAlternative");
		}

		/* *********************************************
 		 * SOUND
		 ********************************************** */
		public void ConfigureSound (float a, float b, float c) {
			Debug.Log ("ConfigureSound");
		}

		public void PlaySound (string path, bool background, bool loop) {
			Debug.Log ("PlaySound");
		}

		public void StopSound (string path) {
			Debug.Log ("StopSound");
		}
	}
}
