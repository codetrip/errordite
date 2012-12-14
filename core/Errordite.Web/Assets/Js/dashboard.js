(function() {

  jQuery(function() {
    var Dashboard;
    return Dashboard = (function() {

      function Dashboard() {
        console.log("init");
        this.issueContainer = $('div#issues');
        this.errorContainer = $('div#errors');
        this.lastPolled = new Date();
      }

      Dashboard.prototype.poll = function() {
        var date;
        console.log("polling");
        date = dashboard.lastPolled;
        dashboard.lastPolled = new Date();
        console.log(date);
        return $.ajax({
          url: "/dashboard/update?lastUpdated=" + date.toLocaleDateString() + ' ' + date.getUTCHours() + ':' + date.getUTCMinutes() + ':' + date.getUTCSeconds(),
          success: function(result) {
            console.log("success");
            if (result.success) {
              return dashboard.bind(result.data);
            } else {
              return dashboard.error();
            }
          },
          error: function() {
            return $this.error();
          },
          dataType: "json",
          complete: dashboard.poll,
          timeout: 10000
        });
      };

      Dashboard.prototype.bind = function(data) {
        var e, i, _i, _j, _len, _len1, _ref, _ref1, _results;
        console.log("binding");
        _ref = data.issues;
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          i = _ref[_i];
          dashboard.issueContainer.prepend(i);
        }
        _ref1 = data.errors;
        _results = [];
        for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
          e = _ref1[_j];
          _results.push(dashboard.errorContainer.prepend(e));
        }
        return _results;
      };

      Dashboard.prototype.error = function() {
        return console.log("error");
      };

      return Dashboard;

    })();
  });

}).call(this);
