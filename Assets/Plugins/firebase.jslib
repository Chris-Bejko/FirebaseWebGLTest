mergeInto(LibraryManager.library, {

  Hello: function () {
    window.alert("Hello, world!");
  },

  ChangeText: function () {
    window.unityInstance.SendMessage();
  }

});