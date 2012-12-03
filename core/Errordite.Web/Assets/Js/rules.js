(function() {
  var ruleCounter;

  ruleCounter = 0;

  jQuery(function() {
    var $body;
    if ($('div#issue, div#addissue').length > 0) {
      $body = $('body');
      Errordite.Rule = (function() {

        Rule.name = 'Rule';

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

        RuleManager.name = 'RuleManager';

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

}).call(this);