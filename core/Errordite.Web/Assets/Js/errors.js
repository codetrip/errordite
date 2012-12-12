(function() {

  jQuery(function() {
    var $root, Error, ErrorProp, init, openedErrors;
    $root = $('section#errors, section#issue, section#errordite-errors').first();
    if ($root.length > 0) {
      init = new Initalisation();
      init.datepicker($root);
      openedErrors = [];
      $root.delegate('ul.tabs li a', 'click', function(e) {
        var $this;
        $this = $(this);
        $this.error = new Error($this);
        $this.error.switchTab();
        return e.preventDefault();
      });
      $root.delegate('td.toggle', 'click', function(e) {
        var $this, error;
        $this = $(this);
        error = new Error($this);
        error.toggle();
        return e.preventDefault();
      });
      if ($('section#issue').length > 0) {
        $root.delegate('.new-rule-match, .rule-match', 'click', function() {
          var $ruleMatch;
          $('.last-selected').removeClass('last-selected');
          $('.remove-rule').hide();
          $ruleMatch = $(this);
          $ruleMatch.addClass('last-selected');
          return $ruleMatch.closest('.prop-val').parent().find('.remove-rule').show().unbind('click').bind('click', function() {
            Errordite.ruleManager.removeRule($ruleMatch.data('ruleId'));
            return $(this).hide();
          }).attr('title', "Click to remove Rule: '" + ($ruleMatch.attr('title')) + "'").tooltip();
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
            _results.push($match.addClass('old-rule-match').removeClass('rule-match').attr('title', 'REMOVED: ' + $match.attr('title')).tooltip());
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
            visualisedHtml = visualisedHtml.replace(regex, "$1<span data-rule-id='" + matchInfo.rule.counter + "' \nclass='ruletip " + (matchInfo.rule.status === 'new' ? 'new-' : '') + "rule-match' \ntitle='" + (matchInfo.rule.description()) + "'>$2</span>$3");
            prevMatchInfo = matchInfo;
          }
          $('span.ruletip').tooltip();
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
          $item = $error.closest('td');
          tabId = $error.data('val');
          $tab = $item.find('div.' + tabId);
          $item.find('ul.tabs li.active').removeClass('active');
          $error.closest('li').addClass('active');
          $tab.siblings('div').addClass('hidden');
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
                $button = $('<button/>').addClass('btn').addClass('btn-rule').addClass('make-rule').text('Create Rule');
                $removeButton = $('<button/>').addClass('btn').addClass('btn-rule').addClass('remove-rule').text('Remove Rule').hide();
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
                  var $this, rule;
                  rule = getRule();
                  $this = $(this);
                  $this.attr('title', "Click to add rule: '" + (rule.description()) + "'");
                  return $this.tooltip();
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

}).call(this);
