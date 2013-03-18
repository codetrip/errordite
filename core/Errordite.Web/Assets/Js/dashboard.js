(function() {

  jQuery(function() {
    var $root, Dashboard, dashboard;
    $root = $('section#dashboard');
    if ($root.length > 0) {
      Dashboard = (function() {

        function Dashboard() {
          this.issueContainer = $('div#issues');
          this.errorContainer = $('div#errors');
          this.lastError = $('input#LastErrorDisplayed').val();
          this.lastIssue = $('input#LastIssueDisplayed').val();
          this.showItems();
        }

        Dashboard.prototype.poll = function() {
          $.ajax({
            url: "/dashboard/update?lastErrorDisplayed=" + dashboard.lastError + '&lastIssueDisplayed=' + dashboard.lastIssue + '&applicationId=' + $('input#ApplicationId').val(),
            success: function(result) {
              console.log("success");
              if (result.success) {
                return dashboard.bind(result.data);
              } else {
                return dashboard.error();
              }
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json",
            complete: function() {
              return setTimeout(dashboard.poll, 10000);
            }
          });
          return true;
        };

        Dashboard.prototype.rendergraph = function() {
          $.ajax({
            url: "/dashboard/getgraphdata?applicationId=" + $('input#ApplicationId').val(),
            success: function(data) {
              return $.jqplot('date-graph', [_.zip(data.x, data.y)], {
                seriesDefaults: {
                  renderer: $.jqplot.LineRenderer
                },
                axes: {
                  xaxis: {
                    renderer: $.jqplot.DateAxisRenderer,
                    tickOptions: {
                      formatString: '%a %#d %b'
                    }
                  },
                  yaxis: {
                    min: 0,
                    tickInterval: _.max(data.y > 3) ? null : 1
                  }
                },
                highlighter: {
                  show: true,
                  sizeAdjust: 7.5
                }
              });
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json"
          });
          return true;
        };

        Dashboard.prototype.bind = function(data) {
          var e, i, _i, _j, _len, _len1, _ref, _ref1;
          console.log("binding");
          _ref = data.issues;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            i = _ref[_i];
            dashboard.issueContainer.prepend(i);
          }
          _ref1 = data.errors;
          for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
            e = _ref1[_j];
            dashboard.errorContainer.prepend(e);
          }
          dashboard.lastError = data.lastErrorDisplayed;
          dashboard.lastIssue = data.lastIssueDisplayed;
          dashboard.showItems();
          return true;
        };

        Dashboard.prototype.error = function() {
          console.log("error");
          return true;
        };

        Dashboard.prototype.showItems = function() {
          this.issueContainer.find('div.boxed-item:hidden').show('slow');
          this.errorContainer.find('div.boxed-item:hidden').show('slow');
          this.purgeItems(this.issueContainer);
          return this.purgeItems(this.errorContainer);
        };

        Dashboard.prototype.purgeItems = function($container) {
          var count, _results;
          count = $container.find(' > div').length;
          _results = [];
          while (count > 100) {
            $container.find(' > div:last-child').remove();
            _results.push(count = $container.find(' > div').length);
          }
          return _results;
        };

        return Dashboard;

      })();
      dashboard = new Dashboard();
      dashboard.rendergraph();
      setTimeout(dashboard.poll, 10000);
      return true;
    }
  });

}).call(this);
