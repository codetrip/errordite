(function() {

  jQuery(function() {
    var $issue, clearErrors, loadTabData, paging, renderHistory, renderReports, setStatus;
    $issue = $('section#issue');
    if ($issue.length > 0) {
      paging = new window.Paging('/issue/errors?Id=' + $issue.find('#IssueId').val() + '&');
      paging.init();
      loadTabData = function($tab) {
        if (!$tab.data('loaded')) {
          if ($tab.data("val") === "reports") {
            renderReports();
          } else if ($tab.data("val") === "history") {
            renderHistory();
          }
          return $tab.data('loaded', true);
        }
      };
      renderReports = function() {
        $('div#date-graph').empty();
        $('div#hour-graph').empty();
        return $.get("/issue/getreportdata?issueId=" + $issue.find('input#IssueId').val() + '&dateRange=' + $issue.find('input#DateRange').val(), function(d) {
          $.jqplot('hour-graph', [d.ByHour.y], {
            seriesDefaults: {
              renderer: $.jqplot.BarRenderer
            },
            axes: {
              xaxis: {
                renderer: $.jqplot.CategoryAxisRenderer,
                ticks: d.ByHour.x
              },
              yaxis: {
                min: 0,
                tickInterval: (_.max(d.ByHour.y)) > 3 ? null : 1
              }
            }
          });
          return $.jqplot('date-graph', [_.zip(d.ByDate.x, d.ByDate.y)], {
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
                tickInterval: (_.max(d.ByDate.y)) > 3 ? null : 1
              }
            },
            highlighter: {
              show: true,
              sizeAdjust: 7.5
            }
          });
        });
      };
      clearErrors = function() {
        return $('div#error-items').clear();
      };
      setStatus = function(status) {
        return $.post('/issue/setstatus', {
          issueId: $issue.find('input#IssueId').val(),
          status: status
        }, function(data) {
          if (data.redirect !== '') {
            return window.location.href = data.redirect;
          } else if (data.success) {
            $issue.find('div#status').text(status);
            Errordite.Notification.show(data.message);
            $issue.find('select#Status').val(status);
            switch (status) {
              case 'Solved':
                $issue.find('li#solved').hide();
                $issue.find('li#ignored').show();
                $issue.find('li#acknowledged').hide();
                return $issue.find('li#fixready').hide();
              case 'Acknowledged':
                $issue.find('li#acknowledged').hide();
                $issue.find('li#ignored').show();
                $issue.find('li#solved').show();
                return $issue.find('li#fixready').show();
              case 'FixReady':
                $issue.find('li#acknowledged').hide();
                $issue.find('li#fixready').hide();
                $issue.find('li#ignored').show();
                return $issue.find('li#solved').show();
              case 'Ignored':
                $issue.find('li#acknowledged').hide();
                $issue.find('li#fixready').hide();
                $issue.find('li#ignored').hide();
                return $issue.find('li#solved').show();
            }
          } else {
            return Errordite.Alert.show('An error occured while trying to update the issue, please try again');
          }
        }).error(function(e) {
          return Errordite.Alert.show('An error occured while trying to update the issue, please try again');
        });
      };
      renderHistory = function() {
        var $node, url;
        $node = $issue.find('table.history tbody');
        url = '/issue/history?IssueId=' + $issue.find('#IssueId').val();
        return $.get(url, function(data) {
          $node.append(data.data);
          return $('div.content').animate({
            scrollTop: 0
          }, 'slow');
        });
      };
      loadTabData($('ul#issue-tabs li.active a.tablink'));
      $issue.delegate('form#reportform', 'submit', function(e) {});
      $issue.delegate('form#reportform', 'submit', function(e) {
        e.preventDefault();
        return renderReports();
      });
      $issue.delegate('.what-if-reprocess', 'click', function(e) {
        e.preventDefault();
        return $(this).closest('form').ajaxSubmit({
          data: {
            WhatIf: true
          },
          success: function(data) {
            var msg;
            $('.reprocess-what-if-msg').remove();
            msg = $('<span/>').addClass('reprocess-what-if-msg').html(data);
            return $(e.currentTarget).after(msg);
          },
          error: function() {
            return Errordite.Alert.show('An error has occured, please try again.');
          }
        });
      });
      $issue.delegate('ul#action-list a.action', 'click', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return setStatus($this.data('val'));
      });
      $issue.delegate('select#Status', 'change', function() {
        var $this;
        $this = $(this);
        if ($this.val() === 'Ignored') {
          return $issue.find('li.inline').removeClass('hidden');
        } else {
          return $issue.find('li.inline').addClass('hidden');
        }
      });
      if ($issue.find('select#Status').val() === 'Ignored') {
        $issue.find('li.inline').removeClass('hidden');
      }
      return $('#issue-tabs .tablink').bind('shown', function(e) {
        return loadTabData($(e.currentTarget));
      });
    }
  });

}).call(this);
