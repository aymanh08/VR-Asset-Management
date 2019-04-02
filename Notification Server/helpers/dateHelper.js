module.exports = {
    getDateTimeString: function () {
      var current = new Date()
      var year = current.getFullYear().toString();
      var month = ("0" + (current.getMonth() + 1)).slice(-2);
      var day = ("0" + current.getDate()).slice(-2);

      var hour = ("0" + current.getHours()).slice(-2);
      var minutes = ("0" + current.getMinutes()).slice(-2);
      var seconds = ("0" + current.getSeconds()).slice(-2);
      return year + '-' + month + '-' + day + ' ' + hour + ':' + minutes + ':' + seconds
    },
    getTimeStamp: function (){
        var current = new Date();
        return current.getTime().toString();
    }
  };