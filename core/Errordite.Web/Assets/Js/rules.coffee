ruleCounter = 0

jQuery -> 

	if $('div#issue, div#addissue').length > 0
		$body = $ 'body'		

		class Errordite.Rule			

			constructor: ($rule) ->
				this.$rule = $rule
				if this.$rule?					
					this.prop = $rule.find('.rule-prop').val()
					this.op = $rule.find('.rule-operator').val()
					this.val = $rule.find('.rule-val').val()
					this.status = if $rule.hasClass('new-rule') or $rule.hasClass('changed-rule') then 'new' else 'saved'										
					this.counter = ruleCounter++
					#both the rule and the $rule have a reference to each other
					$rule.data 'rule', this
					$rule.data 'counter', this.counter
			
			description: () -> 
				"#{this.prop} #{this.op} #{'"' + this.val + '"'}"		

			update: () -> 
				#updates the data from the element
				if this.$rule?
					this.prop = this.$rule.find('.rule-prop').val()
					this.op = this.$rule.find('.rule-operator').val()
					this.val = this.$rule.find('.rule-val').val()
					this.status = if this.$rule.hasClass('new-rule') or this.$rule.hasClass('changed-rule') then 'new' else 'saved'
				

		class Errordite.RuleManager
			
			constructor: () ->				
				this.counter = 0
				this.rules = for ruleEl in $ '#rules-table tr.rule'
					new Errordite.Rule $ ruleEl								

			addRule: (name, op, val) ->
				
				if this.rules.length > 0
					$newRow = $('table#rules-table tr.rule:first').clone()				
					$newRow.insertAfter('table#rules-table tr.rule:last')
				else
					#if there are no rules in the datamodel, we expect there to be a rule available to
					#show (see removeRule)
					$newRow = $('table#rules-table tr.rule:first')
					$newRow.show()

				$newRow.addClass 'new-rule'				

				this.reindex()
				
				$newRow.find(':input').val('')
				$newRow.find('.rule-prop').val(name) if name?
				$newRow.find('.rule-operator').val(op) if op?
				$newRow.find('.rule-val').val(val) if val?

				this.parseRulesForm()
				this.showRuleUpdatesPanel()
				rule = new Errordite.Rule($newRow)
				this.rules.push rule
				rule.$rule.trigger 'ruleadded'
				rule				

			removeRule: ($rule) ->					
						
				if isFinite $rule
					rule = _(this.rules).find (rule) -> 
						rule.counter == $rule
					return false if !rule?
					$rule = rule.$rule
									
				this.parseRulesForm()
				
				this.rules = (rule for rule in this.rules when $rule.data('rule') != rule)											
				$rule.trigger 'remove'
				if this.rules.length > 0
					$rule.remove()
					this.showRuleUpdatesPanel()				
				else 
					#if there is only one rule left, hide it rather than removing it so that there is something there to act as a template when 
					#we add a new one in
					$rule.hide()
					this.hideRuleUpdatesPanel()
					
				this.reindex()

			parseRulesForm: () ->
				$form = $('form#rulesForm', 'form#addIssue')
				$form.removeData("validator")
				$form.removeData("unobtrusiveValidation")
				$.validator.unobtrusive.parse($form)

			reindex: () -> 
				index = 0

				nameToId = (name) -> 
					name.replace(/\.|\[|\]/g, '_')

				$('table#rules-table tbody tr').each (idx, itm) -> 
					$item = $(itm)
					for input in $item.find ':input' when /\[\d*\]/.test input.name
						oldName = input.name
						input.name = input.name.replace /(.*)\[\d*\]/, "$1[#{index}]"												
						for valmsg in $item.find "[data-valmsg-for='#{oldName}']"
							$(valmsg).attr 'data-valmsg-for', input.name
						input.id = nameToId input.name
					index++
			
			showRuleUpdatesPanel: () ->
				$('#rules-adjusted').show()
			
			hideRuleUpdatesPanel: () -> 
				$('#rules-adjusted').hide()

		Errordite.ruleManager = new Errordite.RuleManager()		

		$body.delegate 'button#apply-rule-updates', 'click', (e) ->
			$form = $('form#rulesForm')
			$form.validate()

			if $form.valid()
				$('#apply-rules-confirmation').modal()

			(Tabs.get $ '#issue-tabs').show 'rules'

		$body.delegate 'div#rules a.add', 'click', (e) -> 			
			Errordite.ruleManager.addRule()
			e.preventDefault()

		$body.delegate 'div#rules a.delete', 'click', (e) -> 
			Errordite.ruleManager.removeRule $(this).closest 'tr'			
			e.preventDefault()

		$body.delegate 'tr.rule :input', 'change', () -> 
			$rule = $(this).closest 'tr.rule'
			$rule.data('rule').update()			
			$rule.addClass 'changed-rule'
			$('body').trigger 'changedrule', $rule
			Errordite.ruleManager.showRuleUpdatesPanel()

		if $('table#rules-table tbody tr').length == 1
			$body.find('a.delete').hide()
			$body.find('span.divider').hide()


		