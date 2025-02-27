mergeInto(LibraryManager.library, {

  GetJSON: function (path, objectName, callback, fallback) {
		var parsedPath = Pointer_stringify(path);
		var parsedObjectName= Pointer_stringify(objectName);
		var parsedCallback = Pointer_stringify(callback);
		var parsedFallback = Pointer_stringify(fallback);

		try{
		firebase.database().ref(parsedPath).once('value').then(function(snapshot) {
			window.unityInstance.SendMessage(parsedObjectName, parsedCallback , JSON.stringify(snapshot.val()));


		});

		} catch(error){
			window.unityInstance.SendMessage(parsedObjectName, parsedFallback, "There was an error: " + error.message);
			
		}
		},

	  PushJSON: function(path, value, objectName, callback, fallback) {
        var parsedPath = UTF8ToString(path);
        var parsedValue = UTF8ToString(value);
        var parsedObjectName = UTF8ToString(objectName);
        var parsedCallback = UTF8ToString(callback);
        var parsedFallback = UTF8ToString(fallback);

        try {

            firebase.database().ref(parsedPath).push().set(parsedValue).then(function(unused) {
                window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: " + parsedValue + " was pushed to " + parsedPath);
            });

        } catch (error) {
            window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    },
	PostJSON: function(path, value, objectName, callback, fallback) {
        var parsedPath = UTF8ToString(path);
        var parsedValue = UTF8ToString(value);
        var parsedObjectName = UTF8ToString(objectName);
        var parsedCallback = UTF8ToString(callback);
        var parsedFallback = UTF8ToString(fallback);

        try {

            firebase.database().ref(parsedPath).set(parsedValue).then(function(unused) {
                window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: " + parsedValue + " was posted to " + parsedPath);
            });

        } catch (error) {
            window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    },
    ModifyNumberWithTransaction: function(path, amount, objectName, callback, fallback) {
        var parsedPath = UTF8ToString(path);
        var parsedObjectName = UTF8ToString(objectName);
        var parsedCallback = UTF8ToString(callback);
        var parsedFallback = UTF8ToString(fallback);

        try {

            firebase.database().ref(parsedPath).transaction(function(currentValue) {
                if (!isNaN(currentValue)) {
                    return currentValue + amount;
                } else {
                    return amount;
                }
            }).then(function(unused) {
                window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: transaction run in " + parsedPath);
            });

        } catch (error) {
            window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    },


   SignInUserAnonymously: function (objectName, callback, fallback) {
        var parsedObjectName = UTF8ToString(objectName);
        var parsedCallback = UTF8ToString(callback);
        var parsedFallback = UTF8ToString(fallback);

        try {
            firebase.auth().signInAnonymously().then(function (result) {
                firebase.auth().onAuthStateChanged((user) => {
                if(user){
                    var uid = user.uid;
                    firebase.analytics().logEvent('login');
                    window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: signed up for " + uid);
                }else{
                    window.unityInstance.SendMessage(parsedObjectName, parsedFallback, "Error getting user");
                }
            
                
                });
                window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: signed up for " + result);
            }).catch(function (error) {
                window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
            });

        } catch (error) {
            window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }

 
    
        
    },

    LogEventWithParameter: function(eventName, parameter, parameterValue, objectName, callback, fallback){
        var parsedEventName = UTF8ToString(eventName);
        var parsedParameter = UTF8ToString(parameter);
        var parsedValue = UTF8ToString(parameterValue);
        var parsedObjectName = UTF8ToString(objectName);
        var parsedCallback = UTF8ToString(callback);
        var parsedFallback = UTF8ToString(fallback);
        //let name = parsedParamater;
        //console.log(parsedEventName);
        //console.log([name]);
        //console.log(parsedValue);
        try{
            firebase.analytics().logEvent(parsedEventName,parsedValue);
            window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: logged event: " + parsedEventName);
        }catch(error){
            window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    
    },
     SetUserProperty: function(userPropertySet, objectName, callback, fallback){
        var parsedUserPropertySet = UTF8ToString(userPropertySet);        
        var parsedObjectName = UTF8ToString(objectName);
        var parsedCallback = UTF8ToString(callback);
        var parsedFallback = UTF8ToString(fallback);
        try{
            firebase.analytics().setUserProperties(parsedUserPropertySet);
            window.unityInstance.SendMessage(parsedObjectName, parsedCallback, "Success: set user property : " + parsedUserPropertySet);
        }catch(error){
            window.unityInstance.SendMessage(parsedObjectName, parsedFallback, JSON.stringify(error, Object.getOwnPropertyNames(error)));
        }
    
    },

});