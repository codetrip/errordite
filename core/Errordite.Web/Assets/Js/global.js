(function() {
  var Initialisation, Paging, Spinner, Tabs;

  window.Errordite = {};

  RegExp.escape= function(s) {
	return s.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&');
};;


  Initialisation = (function() {

    function Initialisation() {}

    Initialisation.prototype.init = function(ajax, pagingFunc) {
      var $paging, $tabHolders, controller, paging, tabHolder, _i, _len;
      $('.icon-info').tooltip();
      $('.tool-tip').tooltip();
      $('div.search-box').tooltip();
      $('.dropdown-toggle').dropdown();
      $paging = $('div.paging');
      paging = new Paging(pagingFunc);
      paging.init(ajax);
      $tabHolders = $('.tabs');
      prettyPrint();
      for (_i = 0, _len = $tabHolders.length; _i < _len; _i++) {
        tabHolder = $tabHolders[_i];
        controller = new Tabs(tabHolder);
        $(tabHolder).data('controller', controller);
        controller.init();
      }
      Errordite.Spinner.enable();
      $('body').on('click', 'a.ajax', function(e) {
        e.preventDefault();
        return $.ajax({
          url: this.href,
          type: $(this).hasClass('ajax-post') ? 'post' : 'get',
          success: function(data) {
            return alert(data);
          },
          failure: function() {
            return 'failed';
          }
        });
      });
      return $('body').on('click', '[data-confirm]', function() {
        return confirm($(this).data('confirm'));
      });
    };

    Initialisation.prototype.datepicker = function($root) {
      return $root.find('div#daterange').daterangepicker({
        ranges: {
          Today: ["today", "today"],
          Yesterday: ["yesterday", "yesterday"],
          "Last 7 Days": [
            Date.today().add({
              days: -6
            }), "today"
          ],
          "Last 30 Days": [
            Date.today().add({
              days: -29
            }), "today"
          ],
          "This Month": [Date.today().moveToFirstDayOfMonth(), Date.today().moveToLastDayOfMonth()]
        }
      }, function(start, end) {
        $('#daterange span').html(start.toString('MMMM d, yyyy') + ' - ' + end.toString('MMMM d, yyyy'));
        return $('#daterange input').val(start.toString('u') + '|' + end.toString('u'));
      });
    };

    return Initialisation;

  })();

  Spinner = (function() {

    function Spinner() {}

    Spinner.prototype.disable = function() {
      return $('.spinner').ajaxStart(function() {
        return $(this).hide();
      }).ajaxStop(function() {
        return $(this).hide();
      });
    };

    Spinner.prototype.enable = function() {
      return $('.spinner').ajaxStart(function() {
        return $(this).show();
      }).ajaxStop(function() {
        return $(this).hide();
      });
    };

    return Spinner;

  })();

  /*
  The idea with Tabs is as follows:
   1. each set of tabs headers (identified by a container having the class "tabs") gets initialised with a Tab Manager (instance of "Tabs" class)
   2. the corresponding tab bodies appear somewhere on the page and have ids that correspond to the data-val attribute value of their headers
   3. changing a tab pushes state to the history (this should probably be parameterised - true/false)
   4. if something needs to happen when a tab is shown, you can bind to the "Shown" event on the .tablink element inside the tab header (example in issues.coffee)
   5. to get to a particular Tab Manager, call Tabs.get(), passing any node inside the .tabs element (or the .tabs element itself)
  
  It could do with a little tweaking and polishing to make some of the names line up better and have fewer significant elements but the principle is
  that the tabs get initialised and then we use events for anything instance-specific.
  */


  Tabs = (function() {

    Tabs.get = function(anyNodeInside) {
      var $tabHolder, tabManager;
      $tabHolder = $(anyNodeInside).closest('.tabs');
      if (!$tabHolder.length) {
        return null;
      }
      tabManager = $tabHolder.data('controller');
      if (!(tabManager != null)) {
        tabManager = new Tabs($tabHolder);
        tabManager.init();
        $tabHolder.data('controller', tabManager);
      }
      return tabManager;
    };

    function Tabs(tabHolder) {
      this.node = $(tabHolder);
      this.parentNode = this.node.closest(':has(.tab)');
    }

    Tabs.prototype.show = function(tabName) {
      var $activeNode, $tab, inactiveNode;
      if (this.parentNode.length === 0) {
        return;
      }
      $tab = this.parentNode.find('div#' + tabName);
      if (!$tab.length) {
        return false;
      }
      inactiveNode = this.node.find('li.active');
      inactiveNode.removeClass('active');
      inactiveNode.addClass('inactive');
      $activeNode = $("li:has(a[data-val=" + tabName + "])");
      $activeNode.addClass('active');
      $activeNode.removeClass('inactive');
      this.parentNode.find('div.tab').addClass('hidden');
      $tab.removeClass('hidden');
      return $activeNode.find('.tablink').trigger('shown');
    };

    Tabs.prototype.init = function() {
      var first,
        _this = this;
      if (this.node.data('init') === true) {
        return;
      }
      this.node.data('init', true);
      if (this.parentNode.length === 0) {
        return;
      }
      first = true;
      window.onpopstate = function(evt) {
        if (first) {
          return first = false;
        } else {
          return _this.show(evt.state || _this.node.find('li a [data-val]:first').data('val'));
        }
      };
      return this.node.delegate('li a.tablink', 'click', function(e) {
        var $a, tabName;
        e.preventDefault();
        $a = $(e.currentTarget);
        tabName = $a.data('val');
        _this.show(tabName);
        if (!(window.history.pushState != null)) {
          return;
        }
        return window.history.pushState(tabName, '', $a.attr('href'));
      });
    };

    return Tabs;

  })();

  /*
  Paging is the class responsible for all Paging operations.  It's a bit neither-one-thing nor another
  it is a singleton but of course there could be multiple paging controls on the page.  This means whenever you 
  do something you have to specify which $paging div you are talking about.  A potential improvement could be
  to instantiate a Paging class each time you do something, telling it at this time which one you are talking about.
  */


  Paging = (function() {

    function Paging(changeFunc) {
      var paging;
      paging = this;
      this.currentPage = 0;
      this.currentSize = 0;
      this.changeFunc = changeFunc;
      this.pushState = false;
      this.rootNode = $('body');
      this.baseUrl = this.rootNode.find('input#page-link').val();
      this.contentNode = $('div.content');
      /*
      		Once we've worked out what url we want to navigte to we call navigate.  $paging is the .paging div
      		that holds our paging controls.
      */

      this.navigate = function($paging, url) {
        var $ajaxContainer;
        $ajaxContainer = $paging.closest('.ajax-container');
        if ($ajaxContainer.length) {
          return $.get(url, {}, function(data) {
            return $ajaxContainer.html(data);
          });
        } else {
          return window.location.href = url;
        }
      };
      this.getBaseUrl = function($paging) {
        return $paging.find('input#page-link').val();
      };
      this.init = function() {
        this.rootNode.delegate('input#pgno', 'blur', function(e) {
          var $paging, $this;
          e.preventDefault();
          $this = $(this);
          $paging = $this.closest('.paging');
          if ($this.val() !== $this.data('currentPage')) {
            return paging.navigate($paging, decodeURI(paging.getBaseUrl($paging).replace('[PGNO]', $this.val()).replace('[PGSZ]', $paging.find('select#pgsz').val())));
          }
        });
        this.rootNode.delegate('input#pgno', 'focus', function(e) {
          var $this;
          e.preventDefault();
          $this = $(this);
          return $this.data('currentPage', $this.val());
        });
        this.rootNode.delegate('select#pgsz', 'change', function(e) {
          var $paging, $this, firstItemNumber, newPageNumber;
          e.preventDefault();
          $this = $(this);
          $paging = $this.closest('.paging');
          firstItemNumber = ($paging.find('input#pgno').val() - 1) * $this.data('current') + 1;
          newPageNumber = Math.floor(firstItemNumber / $this.val()) + 1;
          return paging.navigate($paging, decodeURI(paging.getBaseUrl($paging).replace('[PGNO]', newPageNumber).replace('[PGSZ]', $this.val())));
        });
        this.rootNode.delegate('div.pagination a', 'click', function(e) {
          var $paging, $this;
          e.preventDefault();
          $this = $(this);
          $paging = $this.closest('.paging');
          if ($this.hasClass('active') || $this.hasClass('disabled')) {
            return;
          }
          return paging.navigate($paging, $this.attr('href'));
        });
        return this.contentNode.delegate('th.sort a', 'click', function(e) {
          var $paging, $this;
          e.preventDefault();
          $this = $(this);
          $paging = $this.closest('.paging');
          return paging.navigate($paging, $this.attr('href'));
        });
      };
    }

    return Paging;

  })();

  window.Tabs = Tabs;

  window.Paging = Paging;

  window.Initalisation = Initialisation;

  window.Errordite.Spinner = new Spinner();

  jQuery(function() {
    var init;
    init = new Initialisation();
    return init.init(false);
  });

}).call(this);
