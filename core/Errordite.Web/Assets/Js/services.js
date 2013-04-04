(function() {

  jQuery(function() {
    var $root, ServiceManager, serviceManager;
    $root = $("section#services");
    if ($root.length > 0) {
      $root.delegate("a.purge", "click", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to delete all messages?")) {
          return false;
        }
        return serviceManager.deleteMessages($this.data("queue"), $this.data("service"));
      });
      $root.delegate("a.retry", "click", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to retry all messages?")) {
          return false;
        }
        return serviceManager.returnToSource($this.data("queue"), $this.data("service"));
      });
      $root.delegate("select#RavenInstanceId", "change", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return serviceManager.switchInstance($this.val());
      });
      ServiceManager = (function() {

        function ServiceManager() {}

        ServiceManager.prototype.deleteMessages = function() {
          var deleteMessages;
          deleteMessages = function(queueName, serviceName) {};
          $.ajax({
            url: "/system/services/deletemessages",
            data: {
              instanceId: $('select#RavenInstanceId').val(),
              queueName: queueName,
              serviceName: serviceName
            },
            success: function(result) {
              if (result.Success) {
                location.reload();
              } else {
                alert("Failed to delete messages, please try again.");
              }
              return true;
            },
            error: function() {
              return alert("Failed to delete messages, please try again.");
            },
            dataType: "json",
            type: "POST"
          });
          return true;
        };

        ServiceManager.prototype.returnToSource = function(queueName, serviceName) {
          console.log('instance: ' + $('select#RavenInstanceId').val());
          console.log('queue:' + queueName);
          console.log('service: ' + serviceName);
          $.ajax({
            url: "/system/services/returntosource",
            data: {
              instanceId: $('select#RavenInstanceId').val(),
              queueName: queueName,
              serviceName: serviceName
            },
            success: function(result) {
              if (result.Success) {
                location.reload();
              } else {
                alert("Failed to return error messages to their source queue, please try again.");
              }
              return true;
            },
            error: function() {
              return alert("Failed to return error messages to their source queue, please try again.");
            },
            dataType: "json",
            type: "POST"
          });
          return true;
        };

        ServiceManager.prototype.reload = function() {
          window.location = "/system/services?ravenInstanceId=" + $('select#RavenInstanceId').val();
          return true;
        };

        ServiceManager.prototype.switchInstance = function(instanceId) {
          window.location = "/system/services?ravenInstanceId=" + instanceId;
          return true;
        };

        return ServiceManager;

      })();
      serviceManager = new ServiceManager();
      return true;
    }
  });

}).call(this);
