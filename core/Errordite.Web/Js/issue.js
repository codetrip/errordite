(function() {

  jQuery(function() {
    var $issue, loadTabData, renderDistribution, renderErrors;
    $issue = $('div#issue');
    if ($issue.length > 0) {
      $(document).ready(function() {
        return loadTabData($('ul#issue-tabs li.active a.tablink'));
      });
      $issue.delegate('input[type="button"].confirm', 'click', function() {
        var $this;
        $this = $(this);
        if (confirm("Are you sure you want to delete all errors associated with this issue?")) {
          return $.post('/issue/purge', 'issueId=' + $this.attr('data-val'), function(data) {
            renderErrors('/issue/errors?issueId=' + $this.attr('data-val'));
            return $('span#instance-count').text("0");
          });
        }
      });
      loadTabData = function($tab) {
        if (!$tab.data('loaded')) {
          if ($tab.data("val") === "reports") {
            renderDistribution();
          } else if ($tab.data("val") === "errors") {
            renderErrors('/issue/errors?' + $('form#errorsForm').serialize());
          }
          return $tab.data('loaded', true);
        }
      };
      renderDistribution = function() {
        return $.get("/issue/getreportdata?issueId=" + $issue.find('input[type="hidden"]#IssueId').val(), function(data) {
          var d;
          d = $.parseJSON(data.data);
          return $.jqplot('distribution', d.series, {
            seriesDefaults: {
              renderer: $.jqplot.BarRenderer
            },
            axes: {
              xaxis: {
                renderer: $.jqplot.CategoryAxisRenderer,
                ticks: d.ticks
              }
            }
          });
        });
      };
      renderErrors = function(url) {
        var $node;
        $node = $issue.find('div#error-criteria');
        return $.get(url, function(data) {
          var init;
          $node.html(data);
          init = new Initalisation();
          init.datepicker($issue);
          return $('div.content').animate({
            scrollTop: 0
          }, 'slow');
        });
      };
      $issue.delegate('form#errorsForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return renderErrors('/issue/errors?' + $this.serialize());
      });
      $issue.delegate('select#Status', 'change', function() {
        var $this;
        $this = $(this);
        if ($this.val() === 'Ignorable') {
          $issue.find('li.checkbox').removeClass('hidden');
        } else {
          $issue.find('li.checkbox').addClass('hidden');
        }
        return false;
      });
      if ($issue.find('select#Status').val() === 'Ignorable') {
        $issue.find('li.checkbox').removeClass('hidden');
      }
      return $('#issue-tabs .tablink').bind('shown', function(e) {
        return loadTabData($(e.currentTarget));
      });
    }
  });

}).call(this);
