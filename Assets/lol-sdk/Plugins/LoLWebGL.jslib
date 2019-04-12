var LoLWebGL = {
	_GameIsReady: function(
			gameName,
			callbackObject,
			aspectRatio,
			resolution,
			sdkVersion
		) {
		const targetGameObject = Pointer_stringify(callbackObject);
		console.log('GameIsReady() from JS');
		console.log('Sending data to GameObject' + targetGameObject);

		const EVENT = {
			UNITY: {
				PAUSE: 'PauseMessageReceived',
				RESUME: 'ResumeMessageReceived',
				QUESTION_LIST: 'QuestionListReceived',
				LANGUAGE_DEFS: 'LanguageListReceived',
				START_GAME: 'StartGameDataReceived'
			},
			RECEIVED: {
				PAUSE: 'pause',
				RESUME: 'resume',
				QUESTIONS: 'questions',
				LANGUAGE: 'language',
				START: 'start',
				INIT: 'init'
			}
		};

		// const LANGUAGE_PAYLOAD = {
		// 	_meta: {
		// 		maxChars: {
		// 			welcome: 20
		// 		}
		// 	},
		// 	en: {
		// 		welcome: 'welcome'
		// 	},
		// 	es: {
		// 		welcome: "¡Bienvenido!"
		// 	}
		// };

		const START_GAME_PAYLOAD = {
			languageKey: 'en',
			languageName: 'English',
			lastProgress: JSON.stringify({
				gameProvidedPayload: {
					level: 5,
					stars: 5000
				}
			})
		};

		window.addEventListener("message", function (msg) {
			console.log('[PARENT => UNITY]', msg)

			switch (msg.data.messageName) {
				case EVENT.RECEIVED.PAUSE:
					SendMessage(targetGameObject, EVENT.UNITY.PAUSE);
					break;
				case EVENT.RECEIVED.RESUME:
					SendMessage(targetGameObject, EVENT.UNITY.RESUME);
					break;
				case EVENT.RECEIVED.QUESTIONS:
					SendMessage(targetGameObject, EVENT.UNITY.QUESTION_LIST, msg.data.payload);
					break;
				case EVENT.RECEIVED.LANGUAGE:
					SendMessage(targetGameObject, EVENT.UNITY.LANGUAGE_DEFS, msg.data.payload);
					break;
				case EVENT.RECEIVED.START:
					SendMessage(targetGameObject, EVENT.UNITY.START_GAME, msg.data.payload);
					break;
				case 'init':
					break;
				default:
				 console.log('Unhandled message: ' + msg);
			}
		});

		// Calls init on parent (gameframe)
		parent.postMessage({
			message: 'init',
			payload: JSON.stringify({
				aspectRatio: aspectRatio,
				resolution: resolution,
				sdkVersion: sdkVersion
			})
		}, '*');

		setTimeout(function () {
			// Fake Language. Todo: send 'language' event from harness
			// SendMessage(targetGameObject, EVENT.UNITY.LANGUAGE_DEFS,
			// 	JSON.stringify(LANGUAGE_PAYLOAD)
			// );

			// Fake Start: todo: send 'start' event from harness
			// SendMessage(targetGameObject, EVENT.UNITY.START_GAME,
			// 	JSON.stringify(START_GAME_PAYLOAD)
			// );
		}, 1);
	},

	_PostWindowMessage: function (_messageName, _jsonPayload) {
		const messageName = Pointer_stringify(_messageName);
		const jsonPayload = Pointer_stringify(_jsonPayload);
		const payload = {
			message: messageName,
			payload: jsonPayload
		};
		parent.postMessage(payload, '*');
	}
};

// autoAddDeps(LibraryWebSockets, '$webSocketInstances');
mergeInto(LibraryManager.library, LoLWebGL);
