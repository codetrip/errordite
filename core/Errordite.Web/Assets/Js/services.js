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
        return serviceManager.deleteMessages($this.data("service"));
      });
      $root.delegate("a.retry", "click", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to retry all messages?")) {
          return false;
        }
        return serviceManager.returnToSource($this.data("service"));
      });
      $root.delegate("select#RavenInstanceId", "change", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return serviceManager.switchInstance($this.val());
      });
      $root.delegate("a#refresh", "click", function(e) {
        e.preventDefault();
        return serviceManager.reload();
      });
      $root.delegate('a.start-service', 'click', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return serviceManager.serviceControl($this.data('service'), $this.data('start'));
      });
      ServiceManager = (function() {

        function ServiceManager() {}

        ServiceManager.prototype.deleteMessages = function() {
          var deleteMessages;
          deleteMessages = function(serviceName) {};
          $.ajax({
            url: "/system/services/deletemessages",
            data: {
              instanceId: $('select#RavenInstanceId').val(),
              serviceName: serviceName
            },
            success: function(result) {
              if (result.success) {
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

        ServiceManager.prototype.returnToSource = function(serviceName) {
          $.ajax({
            url: "/system/services/returntosource",
            data: {
              instanceId: $('select#RavenInstanceId').val(),
              serviceName: serviceName
            },
            success: function(result) {
              if (result.success) {
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

        ServiceManager.prototype.serviceControl = function(serviceName, start) {
          $.ajax({
            url: "/system/services/servicecontrol",
            data: {
              serviceName: serviceName,
              start: start
            },
            success: function(result) {
              if (result.success) {
                return location.reload();
              } else {
                alert("Service failed to " + (start != null ? start : {
                  "start": "stop"
                }) + ", please try again.");
              }
              return true;
            },
            error: function() {
              return alert("Service failed to " + (start != null ? start : {
                "start": "stop"
              }) + ", please try again.");
            },
            dataType: "json",
            type: "POST"
          });
          return true;
        };

        return ServiceManager;

      })();
      serviceManager = new ServiceManager();
      return true;
    }
  });

}).call(this);
