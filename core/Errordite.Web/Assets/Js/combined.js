(function() {
  var Initialisation, Paging, Spinner, Tabs, ruleCounter;

  window.Errordite = {};

  RegExp.escape= function(s) {
	return s.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&')
};;


  Initialisation = (function() {

    function Initialisation() {}

    Initialisation.prototype.init = function(ajax, pagingFunc) {
      var $paging, $tabHolders, controller, paging, tabHolder, _i, _len;
      $('.icon-info').tooltip();
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

  jQuery(function() {
    var $body, Application, application;
    $body = $('section#applications');
    if ($body.length > 0) {
      application = null;
      $body.delegate('a.delete-application', 'click', function(e) {
        var $this;
        $this = $(this);
        application = new Application($this.closest('tr'));
        application["delete"]();
        return e.preventDefault();
      });
      $body.delegate('a.generate-error', 'click', function(e) {
        var $this;
        $this = $(this);
        new Application($this.closest('tr')).generateError();
        return e.preventDefault();
      });
      return Application = (function() {

        function Application($appEl) {
          this.$appEl = $appEl;
        }

        Application.prototype["delete"] = function() {
          if (window.confirm("Are you sure you want to delete this application, all associated errors will be deleted?")) {
            return this.$appEl.find('form:has(.delete-application)').submit();
          }
        };

        Application.prototype.generateError = function() {
          return this.$appEl.find('form:has(.generate-error)').submit();
        };

        return Application;

      })();
    }
  });

  jQuery(function() {
    var $body;
    $body = $('div#dashboard-errors');
    if ($body.length > 0) {
      return $body.delegate('select#ApplicationId', 'change', function() {
        var $this;
        $this = $(this);
        if ($this.val() !== '') {
          window.location = $this.val();
        }
        return false;
      });
    }
  });

  jQuery(function() {
    var $root, Error, ErrorProp, init, openedErrors;
    $root = $('div#errors, div#issue, div#errordite-errors, div#dashboard').first();
    if ($root.length > 0) {
      openedErrors = [];
      $root.delegate('div#results ul.nav li a', 'click', function(e) {
        var $this;
        $this = $(this);
        $this.error = new Error($this);
        $this.error.switchTab();
        return e.preventDefault();
      });
      $root.delegate('td.error-rowstate', 'click', function(e) {
        var $this, error;
        $this = $(this);
        error = new Error($this);
        error.toggle();
        return e.preventDefault();
      });
      if ($('div#issue').length > 0) {
        $root.delegate('.new-rule-match, .rule-match', 'click', function() {
          var $ruleMatch;
          $('.last-selected').removeClass('last-selected');
          $('.remove-rule').hide();
          $ruleMatch = $(this);
          $ruleMatch.addClass('last-selected');
          return $ruleMatch.closest('.prop-val').parent().find('.remove-rule').show().unbind('click').bind('click', function() {
            Errordite.ruleManager.removeRule($ruleMatch.data('ruleId'));
            return $(this).hide();
          }).attr('title', "Click to remove Rule: '" + ($ruleMatch.attr('title')) + "'");
        });
        $('body').on('changedrule', function(e, rule) {
          var error, _i, _len, _results;
          _results = [];
          for (_i = 0, _len = openedErrors.length; _i < _len; _i++) {
            error = openedErrors[_i];
            _results.push(error.visualiseRules());
          }
          return _results;
        });
        $('body').on('ruleadded', function() {
          var error, _i, _len, _results;
          _results = [];
          for (_i = 0, _len = openedErrors.length; _i < _len; _i++) {
            error = openedErrors[_i];
            _results.push(error.visualiseRules());
          }
          return _results;
        });
      }
      if ($('div#dashboard').length === 0) {
        init = new Initalisation();
        init.datepicker($root);
      }
      $('body').on('remove', 'tr.rule', function() {
        var $match, $tr, id, match, _i, _len, _ref, _results;
        $tr = $(this);
        id = $tr.data('counter');
        _ref = $("[data-rule-id=" + id + "]");
        _results = [];
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          match = _ref[_i];
          $match = $(match);
          if ($match.hasClass('rule-match')) {
            _results.push($match.addClass('old-rule-match').removeClass('rule-match').attr('title', 'REMOVED: ' + $match.attr('title')));
          } else {
            _results.push($match.replaceWith($match.text()));
          }
        }
        return _results;
      });
      /*
      		Represents a property on an error.
      */

      ErrorProp = (function() {

        function ErrorProp($propEl) {
          this.$propEl = $propEl;
          this.propName = $propEl.data('error-attr');
        }

        ErrorProp.prototype.visualiseRules = function() {
          var length, matchInfo, matchInfos, prevMatchInfo, propValText, regex, rule, visualisedHtml, _i, _len;
          if (!this.propName) {
            return null;
          }
          matchInfos = _.flatten((function() {
            var _i, _len, _ref, _results;
            _ref = Errordite.ruleManager.rules;
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              rule = _ref[_i];
              if (rule.prop === this.propName) {
                _results.push(this.getMatchInfos(rule));
              }
            }
            return _results;
          }).call(this));
          matchInfos = _.sortBy(matchInfos, function(matchInfo) {
            return -matchInfo.start;
          });
          propValText = this.$propEl.find('.prop-val').text();
          visualisedHtml = propValText;
          prevMatchInfo = null;
          for (_i = 0, _len = matchInfos.length; _i < _len; _i++) {
            matchInfo = matchInfos[_i];
            if (!(prevMatchInfo != null) || prevMatchInfo.start > matchInfo.end) {
              length = matchInfo.length;
            } else {
              length = prevMatchInfo.start - matchInfo.start;
            }
            regex = RegExp("^([\\S\\s]{" + matchInfo.start + "})([\\S\\s]{" + length + "})([\\S\\s]*)");
            visualisedHtml = visualisedHtml.replace(regex, "$1<span data-rule-id='" + matchInfo.rule.counter + "' \nclass='" + (matchInfo.rule.status === 'new' ? 'new-' : '') + "rule-match' \ntitle='" + (matchInfo.rule.description()) + "'>$2</span>$3");
            prevMatchInfo = matchInfo;
          }
          return this.$propEl.find('.prop-val').html(visualisedHtml);
        };

        ErrorProp.prototype.getMatchInfos = function(rule) {
          var matchInfos, propValText, regex;
          switch (rule.op) {
            case 'Equals':
              regex = RegExp("(^" + (RegExp.escape(rule.val)) + "$)", "g");
              break;
            case 'Contains':
              regex = RegExp("(" + (RegExp.escape(rule.val)) + ")", "g");
              break;
            case 'StartsWith':
              regex = RegExp("(^" + (RegExp.escape(rule.val)) + ")", "g");
              break;
            case 'EndsWith':
              regex = RegExp("(" + (RegExp.escape(rule.val)) + "$)", "g");
              break;
            case 'RegexMatches':
              regex = RegExp("(" + rule.val + ")", "g");
          }
          matchInfos = [];
          if (regex) {
            propValText = this.$propEl.find('.prop-val').text();
            propValText.replace(regex, function(m, p1, offset) {
              matchInfos.push({
                start: offset,
                length: p1.length,
                end: offset + p1.length,
                match: p1,
                rule: rule
              });
              return null;
            });
          }
          return matchInfos;
        };

        return ErrorProp;

      })();
      /*
      		Represents an error (either within an issue or not).
      */

      return Error = (function() {

        function Error($errorEl) {
          this.$errorEl = $errorEl;
          this.$detailsEl = this.$errorEl.closest('tr').next();
        }

        Error.prototype.switchTab = function() {
          var $error, $item, $tab, tabId;
          $error = this.$errorEl;
          $item = $error.closest('td.error-item');
          tabId = $error.data('val');
          $tab = $item.find('div#' + tabId);
          $item.find('ul.nav li.ui-tabs-selected').removeClass('ui-tabs-selected');
          $error.closest('li').addClass('ui-tabs-selected');
          $tab.siblings().addClass('hidden');
          return $tab.removeClass('hidden');
        };

        Error.prototype.visualiseRules = function() {
          var errorProp, _i, _len, _ref, _results;
          _ref = (function() {
            var _j, _len, _ref, _results1;
            _ref = this.$detailsEl.find("[data-error-attr]");
            _results1 = [];
            for (_j = 0, _len = _ref.length; _j < _len; _j++) {
              errorProp = _ref[_j];
              _results1.push(new ErrorProp($(errorProp)));
            }
            return _results1;
          }).call(this);
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            errorProp = _ref[_i];
            _results.push(errorProp.visualiseRules());
          }
          return _results;
        };

        Error.prototype.toggle = function() {
          var $details, $error, error;
          error = this;
          $error = this.$errorEl;
          $details = this.$detailsEl;
          if ($error.hasClass('expanded')) {
            $error.removeClass('expanded');
            $error.addClass('collapsed');
          } else {
            $error.removeClass('collapsed');
            $error.addClass('expanded');
            if ((Errordite.ruleManager != null) && !$error.data('rules-visualised')) {
              openedErrors.push(this);
              $error.data('rules-visualised', true);
              $details.find('[data-error-attr]').each(function() {
                var $button, $buttons, $errorAttr, $removeButton, $textSpan, getRule, propVal;
                $errorAttr = $(this);
                propVal = $errorAttr.text();
                if ((propVal.trim != null) && !propVal.trim()) {
                  return;
                }
                $buttons = $('<span class="rule-controls"/>').css({
                  display: 'inline'
                }).hide();
                $button = $('<button/>').addClass('btn').addClass('btn-mini').addClass('make-rule').text('Create Rule');
                $removeButton = $('<button/>').addClass('btn').addClass('btn-mini').addClass('remove-rule').text('Remove Rule').hide();
                $buttons.append($button, $removeButton);
                $errorAttr.on('mouseenter', function() {
                  return $buttons.show();
                });
                $errorAttr.on('mouseleave', function() {
                  return $buttons.hide();
                });
                $textSpan = $('<span/>').addClass('prop-val').text(propVal);
                $errorAttr.html($textSpan);
                $errorAttr.append($buttons);
                getRule = function() {
                  var endTextRange, propValSpan, rangeComparison, rule, selectedRange, selection, startTextRange;
                  rule = new Errordite.Rule();
                  rule.prop = $errorAttr.data('error-attr');
                  /*
                  								IE8 and lower do not support window.getSelection.  Tried using IERange (google it) to fill in the
                  								gaps but couldn't be bothered to get it to work properly. Rainy day job (or not at all).
                  */

                  if (!(window.getSelection != null)) {
                    rule.op = 'Equals';
                    return rule.val = propVal;
                  } else {
                    selection = window.getSelection();
                    selectedRange = selection.getRangeAt(0);
                    propValSpan = $errorAttr.find('.prop-val');
                    /*
                    									Because of the messing about with the text we do in terms of inserting "rule-match" spans
                    									we can't just use the whole contents as the text range for comparison; instead we need
                    									to separately get the 1st and last.
                    */

                    startTextRange = document.createRange();
                    startTextRange.selectNodeContents(propValSpan.contents()[0]);
                    endTextRange = document.createRange();
                    endTextRange.selectNodeContents(propValSpan.contents().last()[0]);
                    rangeComparison = {
                      start: selectedRange.compareBoundaryPoints(Range.START_TO_START, startTextRange),
                      end: selectedRange.compareBoundaryPoints(Range.END_TO_END, endTextRange)
                    };
                    if (selection.toString() === '' || rangeComparison.start < 0 || rangeComparison.end > 0) {
                      rule.val = propVal;
                      rule.op = 'Equals';
                    } else {
                      rule.val = selectedRange.toString();
                      if (rangeComparison.start === 0) {
                        if (rangeComparison.end === 0) {
                          rule.op = 'Equals';
                        } else {
                          rule.op = 'StartsWith';
                        }
                      } else {
                        if (rangeComparison.end === 0) {
                          rule.op = 'EndsWith';
                        } else {
                          rule.op = 'Contains';
                        }
                      }
                    }
                    return rule;
                  }
                };
                $button.on('mouseenter', function() {
                  var rule;
                  rule = getRule();
                  return $(this).attr('title', "Click to add Rule: '" + (rule.description()) + "'");
                });
                return $button.on('click', function() {
                  var errorProp, newRule, rule;
                  rule = getRule();
                  newRule = Errordite.ruleManager.addRule(rule.prop, rule.op, rule.val);
                  errorProp = new ErrorProp($button.closest('[data-error-attr]'));
                  return errorProp.visualiseRules();
                });
              });
              this.visualiseRules();
            }
          }
          return $details.toggle();
        };

        return Error;

      })();
    }
  });

  jQuery(function() {
    var $body, Group, group;
    $body = $('section#groups');
    if ($body.length > 0) {
      group = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.group = new Group($this.closest('form'));
        this.group["delete"]();
        return false;
      });
      return Group = (function() {

        function Group($form) {
          this.$form = $form;
        }

        Group.prototype["delete"] = function() {
          if (window.confirm("Are you sure you want to delete this group?")) {
            return this.$form.submit();
          }
        };

        return Group;

      })();
    }
  });

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
        url = '/issue/errors?' + $('#errorsForm').serialize();
        $node = $issue.find('#error-criteria');
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

  jQuery(function() {
    var $activeModal, $root, init, maybeEnableBatchStatus;
    $root = $('section#issues');
    $activeModal = null;
    if ($root.length > 0) {
      init = new Initalisation();
      init.datepicker($root);
      $root.delegate('form#actionForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return $.post($this.attr('action'), $this.serialize(), function(data) {
          return window.location.reload();
        }).error(function(e) {
          if ($activeModal != null) {
            $activeModal.find('div.alert').removeClass('hidden');
            return $activeModal.find("div.alert h4").text("An error occurred, please close the modal window and try again.");
          }
        });
      });
      $root.delegate('ul.dropdown-menu li input', 'click', function(e) {
        return e.stopPropagation();
      });
      $root.delegate('ul.dropdown-menu li a', 'click', function(e) {
        e.preventDefault();
        return $(this).closest('ul').find('li :checkbox').prop('checked', true);
      });
      $root.delegate('ul.dropdown-menu li', 'click', function(e) {
        var $chk, $this;
        $this = $(this);
        $chk = $this.closest('li').children('input');
        $chk.attr('checked', !$chk.attr('checked'));
        return false;
      });
      $root.delegate('ul#action-list ul.dropdown-menu li a', 'click', function() {
        var $modal, $this;
        $this = $(this);
        $modal = $root.find('div#' + $this.attr('data-val-modal'));
        if ($modal === null) {
          return null;
        }
        $root.find('input[type="hidden"]#Action').val($modal.attr("id"));
        $modal.find('.batch-issue-count').text($(':checkbox:checked[name=issueIds]').length);
        $modal.find('.batch-issue-plural').toggle($(':checkbox:checked[name=issueIds]').length > 1);
        if ($modal.find('.batch-issue-status').length > 0) {
          $modal.find('.batch-issue-status').text($this.attr('data-val-status'));
          $root.find('input[type="hidden"]#Status').val($this.attr('data-val-status').replace(' ', ''));
        }
        $activeModal = $modal;
        return $modal.modal();
      });
      $('th :checkbox').on('click', function() {
        $(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'));
        return maybeEnableBatchStatus();
      });
      maybeEnableBatchStatus = function() {
        return $('ul#action-list').toggle(!!$(':checkbox:checked[name=issueIds]').length);
      };
      $root.delegate(':checkbox[name=issueIds]', 'click', function() {
        return maybeEnableBatchStatus();
      });
      return maybeEnableBatchStatus();
    }
  });

  ruleCounter = 0;

  jQuery(function() {
    var $body;
    if ($('section#issue, section#addissue').length > 0) {
      $body = $('body');
      Errordite.Rule = (function() {

        function Rule($rule) {
          this.$rule = $rule;
          if (this.$rule != null) {
            this.prop = $rule.find('.rule-prop').val();
            this.op = $rule.find('.rule-operator').val();
            this.val = $rule.find('.rule-val').val();
            this.status = $rule.hasClass('new-rule') || $rule.hasClass('changed-rule') ? 'new' : 'saved';
            this.counter = ruleCounter++;
            $rule.data('rule', this);
            $rule.data('counter', this.counter);
          }
        }

        Rule.prototype.description = function() {
          return "" + this.prop + " " + this.op + " " + ('"' + this.val + '"');
        };

        Rule.prototype.update = function() {
          if (this.$rule != null) {
            this.prop = this.$rule.find('.rule-prop').val();
            this.op = this.$rule.find('.rule-operator').val();
            this.val = this.$rule.find('.rule-val').val();
            return this.status = this.$rule.hasClass('new-rule') || this.$rule.hasClass('changed-rule') ? 'new' : 'saved';
          }
        };

        return Rule;

      })();
      Errordite.RuleManager = (function() {

        function RuleManager() {
          var ruleEl;
          this.counter = 0;
          this.rules = (function() {
            var _i, _len, _ref, _results;
            _ref = $('#rules-table tr.rule');
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              ruleEl = _ref[_i];
              _results.push(new Errordite.Rule($(ruleEl)));
            }
            return _results;
          })();
        }

        RuleManager.prototype.addRule = function(name, op, val) {
          var $newRow, rule;
          if (this.rules.length > 0) {
            $newRow = $('table#rules-table tr.rule:first').clone();
            $newRow.insertAfter('table#rules-table tr.rule:last');
          } else {
            $newRow = $('table#rules-table tr.rule:first');
            $newRow.show();
          }
          $newRow.addClass('new-rule');
          this.reindex();
          $newRow.find(':input').val('');
          if (name != null) {
            $newRow.find('.rule-prop').val(name);
          }
          if (op != null) {
            $newRow.find('.rule-operator').val(op);
          }
          if (val != null) {
            $newRow.find('.rule-val').val(val);
          }
          this.parseRulesForm();
          this.showRuleUpdatesPanel();
          rule = new Errordite.Rule($newRow);
          this.rules.push(rule);
          rule.$rule.trigger('ruleadded');
          return rule;
        };

        RuleManager.prototype.removeRule = function($rule) {
          var rule;
          if (isFinite($rule)) {
            rule = _(this.rules).find(function(rule) {
              return rule.counter === $rule;
            });
            if (!(rule != null)) {
              return false;
            }
            $rule = rule.$rule;
          }
          this.parseRulesForm();
          this.rules = (function() {
            var _i, _len, _ref, _results;
            _ref = this.rules;
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              rule = _ref[_i];
              if ($rule.data('rule') !== rule) {
                _results.push(rule);
              }
            }
            return _results;
          }).call(this);
          $rule.trigger('remove');
          if (this.rules.length > 0) {
            $rule.remove();
            this.showRuleUpdatesPanel();
          } else {
            $rule.hide();
            this.hideRuleUpdatesPanel();
          }
          return this.reindex();
        };

        RuleManager.prototype.parseRulesForm = function() {
          var $form;
          $form = $('form#rulesForm', 'form#addIssue');
          $form.removeData("validator");
          $form.removeData("unobtrusiveValidation");
          return $.validator.unobtrusive.parse($form);
        };

        RuleManager.prototype.reindex = function() {
          var index, nameToId;
          index = 0;
          nameToId = function(name) {
            return name.replace(/\.|\[|\]/g, '_');
          };
          return $('table#rules-table tbody tr').each(function(idx, itm) {
            var $item, input, oldName, valmsg, _i, _j, _len, _len1, _ref, _ref1;
            $item = $(itm);
            _ref = $item.find(':input');
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              input = _ref[_i];
              if (!(/\[\d*\]/.test(input.name))) {
                continue;
              }
              oldName = input.name;
              input.name = input.name.replace(/(.*)\[\d*\]/, "$1[" + index + "]");
              _ref1 = $item.find("[data-valmsg-for='" + oldName + "']");
              for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
                valmsg = _ref1[_j];
                $(valmsg).attr('data-valmsg-for', input.name);
              }
              input.id = nameToId(input.name);
            }
            return index++;
          });
        };

        RuleManager.prototype.showRuleUpdatesPanel = function() {
          return $('#rules-adjusted').show();
        };

        RuleManager.prototype.hideRuleUpdatesPanel = function() {
          return $('#rules-adjusted').hide();
        };

        return RuleManager;

      })();
      Errordite.ruleManager = new Errordite.RuleManager();
      $body.delegate('button#apply-rule-updates', 'click', function(e) {
        var $form;
        $form = $('form#rulesForm');
        $form.validate();
        if ($form.valid()) {
          $('#apply-rules-confirmation').modal();
        }
        return (Tabs.get($('#issue-tabs'))).show('rules');
      });
      $body.delegate('div#rules a.add', 'click', function(e) {
        Errordite.ruleManager.addRule();
        return e.preventDefault();
      });
      $body.delegate('div#rules a.delete', 'click', function(e) {
        Errordite.ruleManager.removeRule($(this).closest('tr'));
        return e.preventDefault();
      });
      $body.delegate('tr.rule :input', 'change', function() {
        var $rule;
        $rule = $(this).closest('tr.rule');
        $rule.data('rule').update();
        $rule.addClass('changed-rule');
        $('body').trigger('changedrule', $rule);
        return Errordite.ruleManager.showRuleUpdatesPanel();
      });
      if ($('table#rules-table tbody tr').length === 1) {
        $body.find('a.delete').hide();
        return $body.find('span.divider').hide();
      }
    }
  });

  jQuery(function() {
    var $body, User, user;
    $body = $('section#users');
    if ($body.length > 0) {
      user = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.user = new User($this.closest('form'));
        this.user["delete"]();
        return false;
      });
      return User = (function() {

        function User($form) {
          this.$form = $form;
        }

        User.prototype["delete"] = function() {
          if (window.confirm("Are you sure you want to delete this user, any issues assigned to this user will be assigned to you!")) {
            return this.$form.submit();
          }
        };

        return User;

      })();
    }
  });

  jQuery(function() {
    var $body;
    $body = $('div#organisations');
    if ($body.length > 0) {
      $body.delegate('form#suspendForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return $.post($this.attr('action'), $this.serialize(), function(data) {
          return window.location.reload();
        });
      });
      $body.delegate('a.suspend', 'click', function(e) {
        var $modal, $this;
        e.preventDefault();
        $this = $(this);
        $modal = $body.find('div#suspend-modal');
        if ($modal === null) {
          return null;
        }
        $modal.find('input[type=hidden]').val($this.data('val'));
        return $modal.modal();
      });
      $body.delegate('input[type=submit].activate', 'click', function(e) {
        var $this;
        $this = $(this);
        if (confirm("are you sure you want to activate this organisation?")) {
          return true;
        }
        e.preventDefault();
        return false;
      });
      return $body.delegate('input[type=submit].delete', 'click', function(e) {
        var $this;
        $this = $(this);
        if (confirm("are you sure you want to delete this organisation, all data will be permenantly deleted?")) {
          return true;
        }
        e.preventDefault();
        return false;
      });
    }
  });

  window.Errordite = {};

}).call(this);
