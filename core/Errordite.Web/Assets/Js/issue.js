(function() {

  jQuery(function() {
    var $issue, loadTabData, renderErrors, renderReports, setReferenceLink;
    $issue = $('section#issue');
    if ($issue.length > 0) {
      setReferenceLink = function() {
        var input, reference;
        input = $(':input[name=Reference]');
        reference = input.val();
        $('#reference-link').empty();
        if (/^https?:\/\//.test(reference)) {
          return $('#reference-link').html($('<a>').attr('href', reference).attr('target', '_blank').text('link'));
        }
      };
      loadTabData = function($tab) {
        if (!$tab.data('loaded')) {
          if ($tab.data("val") === "reports") {
            renderReports();
          } else if ($tab.data("val") === "errors") {
            renderErrors();
          }
          return $tab.data('loaded', true);
        }
      };
      renderReports = function() {
        return $.get("/issue/getreportdata?issueId=" + $issue.find('#IssueId').val(), function(d) {
          $.jqplot('hour-graph', [d.ByHour.y], {
            seriesDefaults: {
              renderer: $.jqplot.BarRenderer
            },
            axes: {
              xaxis: {
                renderer: $.jqplot.CategoryAxisRenderer,
                ticks: d.ByHour.x
              }
            }
          });
          if ((d.ByDate != null) && d.ByDate.x.length && d.ByDate.y.length) {
            return $.jqplot('date-graph', [_.zip(d.ByDate.x, d.ByDate.y)], {
              seriesDefaults: {
                renderer: $.jqplot.LineRenderer
              },
              axes: {
                xaxis: {
                  renderer: $.jqplot.DateAxisRenderer,
                  tickOptions: {
                    formatString: '%a %#d %b %y'
                  }
                },
                yaxis: {
                  min: 0
                }
              },
              highlighter: {
                show: true,
                sizeAdjust: 7.5
              }
            });
          } else {
            return $('#date-graph-box').hide();
          }
        });
      };
      renderErrors = function() {
        var $node, url;
        $node = $issue.find('#error-items');
        url = '/issue/errors?IssueId=' + $issue.find('#IssueId').val();
        console.log(url);
        return $.get(url, function(data) {
          $node.html(data);
          return $('div.content').animate({
            scrollTop: 0
          }, 'slow');
        });
      };
      loadTabData($('ul#issue-tabs li.active a.tablink'));
      setReferenceLink();
      $issue.delegate(':input[name=Reference]', 'change', setReferenceLink);
      $issue.delegate('input[type="button"].confirm', 'click', function() {
        var $this;
        $this = $(this);
        if (confirm("Are you sure you want to delete all errors associated with this issue?")) {
          return $.post('/issue/purge', 'issueId=' + $this.attr('data-val'), function(data) {
            renderErrors();
            return $('span#instance-count').text("0");
          });
        }
      });
      $issue.delegate('form#errorsForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return renderErrors();
      });
      $issue.delegate('select#Status', 'change', function() {
        var $this;
        $this = $(this);
        if ($this.val() === 'Ignorable') {
          return $issue.find('li.checkbox').removeClass('hidden');
        } else {
          return $issue.find('li.checkbox').addClass('hidden');
        }
      });
      if ($issue.find('select#Status').val() === 'Ignorable') {
        $issue.find('li.checkbox').removeClass('hidden');
      }
      $('#issue-tabs .tablink').bind('shown', function(e) {
        return loadTabData($(e.currentTarget));
      });
      return $issue.delegate('.sort a[data-pgst]', 'click', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        $('#pgst').val($this.data('pgst'));
        $('#pgsd').val($this.data('pgsd'));
        renderErrors();
        return false;
      });
    }
  });

}).call(this);
