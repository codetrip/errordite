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
      $('body').on('remove', 'tr.rule', function(e) {
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
            _results.push($match.replaceWith(_.escape($match.text())));
          }
        }
        return _results;
      });
      /*
      		Represents a property on an error.
      */

      ErrorProp = (function() {

        function ErrorProp($propEl) {
          var _this = this;
          this.$propEl = $propEl;
          this.propName = $propEl.data('error-attr');
          this.$propEl.delegate('.new-rule-match, .rule-match', 'click', function(e) {
            var $ruleMatch;
            $('.last-selected').removeClass('last-selected');
            $('.remove-rule').hide();
            $ruleMatch = $(e.currentTarget);
            _this.selectRule($ruleMatch.data('rule-id'));
            return e.doNotUnselect = true;
          });
        }

        ErrorProp.prototype.selectRule = function(ruleId) {
          var $ruleMatches;
          $ruleMatches = this.$propEl.find('.new-rule-match, .rule-match').filter("[data-rule-id=" + ruleId + "]");
          $ruleMatches.addClass('last-selected');
          return this.$propEl.find('.remove-rule').show().unbind('click').bind('click', function(e) {
            Errordite.ruleManager.removeRule(ruleId);
            $(this).closest('.rule-controls').addClass('hide');
            $(this).hide();
            return e.stopPropagation();
          }).attr('title', "Click to remove Rule: '" + ($ruleMatches.attr('title')) + "'");
        };

        ErrorProp.prototype.visualiseRules = function() {
          var gapToPrev, i, length, matchInfo, matchInfos, prevMatchInfo, propValText, regex, rule, visualisedHtml, _i, _len;
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
          prevMatchInfo = null;
          visualisedHtml = matchInfos.length === 0 ? _.escape(propValText) : propValText;
          i = 0;
          for (_i = 0, _len = matchInfos.length; _i < _len; _i++) {
            matchInfo = matchInfos[_i];
            if (!(!(prevMatchInfo != null) || matchInfo.end < prevMatchInfo.start)) {
              continue;
            }
            length = matchInfo.length;
            gapToPrev = prevMatchInfo != null ? prevMatchInfo.start - matchInfo.end : visualisedHtml.length - matchInfo.end;
            regex = RegExp("^([\\S\\s]{" + matchInfo.start + "})([\\S\\s]{" + length + "})([\\S\\s]{" + gapToPrev + "})([\\S\\s]*)");
            visualisedHtml = visualisedHtml.replace(regex, function(m, beforeMatch, matchedBit, gapToPrev, prevAndAfter, offset) {
              var first, last;
              first = i === 0;
              last = ++i === matchInfos.length;
              return "" + (last ? _.escape(beforeMatch) : beforeMatch) + "<span data-rule-id='" + matchInfo.rule.counter + "' \nclass='ruletip " + (matchInfo.rule.status === 'new' ? 'new-' : '') + "rule-match' \ntitle='" + (matchInfo.rule.description()) + "'>" + (_.escape(matchedBit)) + "</span>" + (_.escape(gapToPrev)) + prevAndAfter;
            });
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

        Error.prototype.getControls = function(isMultiLine) {
          /*
          				The result of this is like <div class=rule-controls><div class=buttons>buttons</div><div>&nbsp;</div></div>
          				The purpose of the div with just a space is to allow us to have some space between the cursor and the controls
          				panel but without having the mouseleave event trigger if we are at the top of the stack trace area.s
          */

          var $button, $buttons, $removeButton, ret;
          $button = $('<button/>').addClass('btn').addClass('btn-rule').addClass('make-rule').text('Create Rule');
          $removeButton = $('<button/>').addClass('btn').addClass('btn-rule').addClass('remove-rule').text('Remove Rule').hide();
          $buttons = $('<div/>').addClass('rule-controls').addClass('hide').append($('<div/>').addClass('buttons').append($button, $removeButton));
          if (isMultiLine) {
            $buttons.append($('<div/>').html('&nbsp;'));
          }
          return ret = {
            $button: $button,
            $removeButton: $removeButton,
            $buttons: $buttons
          };
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
                var $button, $buttons, $errorAttr, $textSpan, addOffset, controls, getRule, isMultiLine, propVal;
                $errorAttr = $(this);
                isMultiLine = $errorAttr.data('error-attr') === 'StackTrace';
                propVal = $errorAttr.text();
                if ((propVal.trim != null) && !propVal.trim()) {
                  return;
                }
                controls = error.getControls(isMultiLine);
                $button = controls.$button;
                $buttons = controls.$buttons;
                $errorAttr.on('mouseenter', function() {
                  if (!isMultiLine) {
                    return $buttons.removeClass('hide');
                  }
                });
                $errorAttr.on('mouseleave', function() {
                  $buttons.addClass('hide');
                  return $errorAttr.unbind('mousemove');
                });
                $textSpan = $('<span/>').addClass('prop-val').text(propVal);
                $errorAttr.html($textSpan);
                $errorAttr.append($buttons);
                $errorAttr.on('click', function(e) {
                  if (!e.doNotUnselect) {
                    return $('.last-selected').removeClass('last-selected');
                  }
                });
                if (isMultiLine) {
                  $buttons.css({
                    position: 'absolute'
                  });
                  $errorAttr.on('click', function(e) {
                    addOffset(e);
                    $buttons.removeClass('hide');
                    $buttons.addClass('floating');
                    return $buttons.css({
                      top: e.offsetY - 35,
                      left: e.offsetX - 48
                    });
                  });
                } else {
                  $buttons.addClass('inline');
                }
                addOffset = function(event) {
                  var element;
                  element = event.currentTarget;
                  if (!event.offsetX) {
                    event.offsetX = event.pageX - $(element).offset().left;
                    return event.offsetY = event.pageY - $(element).offset().top;
                  }
                };
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
                    rule.val = propVal;
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
                  }
                  return rule;
                };
                $button.on('mouseenter', function() {
                  var $this, rule;
                  rule = getRule();
                  $this = $(this);
                  return $this.attr('title', "Click to add rule: '" + (rule.description()) + "'");
                });
                return $button.on('click', function(e) {
                  var errorProp, newRule, rule;
                  rule = getRule();
                  newRule = Errordite.ruleManager.addRule(rule.prop, rule.op, rule.val);
                  errorProp = new ErrorProp($button.closest('[data-error-attr]'));
                  errorProp.visualiseRules();
                  e.stopPropagation();
                  errorProp.selectRule(newRule.counter);
                  if (document.selection) {
                    return document.selection.empty();
                  }
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
