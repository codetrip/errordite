jQuery -> 
	$root = $('section#errors, section#issue, section#errordite-errors').first()	

	if $root.length > 0		
		
		init = new Initalisation()
		init.datepicker($root);

		#TODO: we should reset this when we page
		openedErrors = []		

		$root.delegate('ul.tabs li a', 'click', (e) -> 
			$this = $ this
			$this.error = new Error $this
			$this.error.switchTab()
			e.preventDefault()
		)

		$root.delegate('td.toggle', 'click', (e) -> 
			$this = $ this
			error = new Error $this
			error.toggle();		
			e.preventDefault()
		)

		if $('section#issue').length > 0
			$root.delegate '.new-rule-match, .rule-match', 'click', () ->				
				$('.last-selected').removeClass 'last-selected'
				$('.remove-rule').hide()
				$ruleMatch = $ this
				$ruleMatch.addClass 'last-selected'			
				$ruleMatch.closest('.prop-val').parent().find('.remove-rule')
					.show()
					.unbind('click')
					.bind 'click', () -> 
						Errordite.ruleManager.removeRule $ruleMatch.data 'ruleId'
						$(this).hide()
					.attr('title', "Click to remove Rule: '#{$ruleMatch.attr('title')}'")					

			$('body').on 'changedrule', (e, rule) -> 
				error.visualiseRules() for error in openedErrors
			$('body').on 'ruleadded', ->
				error.visualiseRules() for error in openedErrors	

		$('body').on 'remove', 'tr.rule', () ->
			$tr = $ this
			id = $tr.data 'counter'
			#if removing the rule means that the visualisation can now be better, it won't be happen because we're just
			#dealing with the one rule that was removed...
			for match in $ "[data-rule-id=#{id}]"
				$match = $ match
				if $match.hasClass 'rule-match'
					$match.addClass('old-rule-match')
						.removeClass('rule-match')
						.attr('title', 'REMOVED: ' + $match.attr 'title')							
				else					
					$match.replaceWith $match.text()

		###
		Represents a property on an error.
		###
		class ErrorProp
			constructor: ($propEl) ->
				this.$propEl = $propEl
				this.propName = $propEl.data 'error-attr'

			visualiseRules: ->
				return null if not this.propName
				
				#get every match info for every rule works on this property
				matchInfos = _.flatten (this.getMatchInfos rule for rule in Errordite.ruleManager.rules when rule.prop == this.propName)
				#sort by position, working backwards (this means we can modify the string starting at the right 
				#without affecting the position required for subsequent (i.e. "earlier") matches)
				matchInfos = _.sortBy matchInfos, (matchInfo) -> -matchInfo.start

				propValText = this.$propEl.find('.prop-val').text()
				visualisedHtml = propValText

				prevMatchInfo = null								

				for matchInfo in matchInfos		
					#if the previous match info overlaps with the current one at all, we will just display
					#the portion of the current one that comes before the previous one.  This is not ideal 
					#but at least it works for us 				
					if !prevMatchInfo? or prevMatchInfo.start > matchInfo.end 
						length = matchInfo.length 
					else 
						length = prevMatchInfo.start - matchInfo.start
					
					#see http://stackoverflow.com/questions/1068280/javascript-regex-multiline-flag-doesnt-work
					#[\S\s] is a hack to allow us to match newline characters (class of characters or its negation)
					regex = ///^([\S\s]{#{matchInfo.start}})([\S\s]{#{length}})([\S\s]*)///					
					visualisedHtml = visualisedHtml.replace(regex, 
							"""
							$1<span data-rule-id='#{matchInfo.rule.counter}' 
							class='ruletip #{if matchInfo.rule.status == 'new' then 'new-' else ''}rule-match' 
							title='#{matchInfo.rule.description()}'>$2</span>$3
							""") 
					prevMatchInfo = matchInfo					
				
				this.$propEl.find('.prop-val').html(visualisedHtml)

			getMatchInfos: (rule) ->
				switch rule.op
					when 'Equals' then regex = ///(^#{RegExp.escape rule.val}$)///g
					when 'Contains' then regex = ///(#{RegExp.escape rule.val})///g
					when 'StartsWith' then regex = ///(^#{RegExp.escape rule.val})///g
					when 'EndsWith' then regex = ///(#{RegExp.escape rule.val}$)///g
					when 'RegexMatches' then regex = ///(#{rule.val})///g	

				matchInfos = []
				if regex					
					propValText = this.$propEl.find('.prop-val').text()		

					propValText.replace regex, (m, p1, offset) ->
						#probably should be using the match function, rather than replace (since we're not actually replacing at this point)
						matchInfos.push 
							start: offset,
							length: p1.length,
							end: offset + p1.length,
							match: p1,
							rule: rule
						null					
				matchInfos					

		###
		Represents an error (either within an issue or not).
		###
		class Error			

			constructor: ($errorEl) -> 
				this.$errorEl = $errorEl
				this.$detailsEl = this.$errorEl.closest('tr').next()				

			switchTab: () ->
				$error = this.$errorEl
				$item = $error.closest('td')
				tabId = $error.data('val')
				$tab = $item.find('div.' + tabId)
				$item.find('ul.tabs li.active').removeClass('active')
				$error.closest('li').addClass('active')
				$tab.siblings('div').addClass('hidden')
				$tab.removeClass('hidden')								

			visualiseRules: ->
				for errorProp in (new ErrorProp $ errorProp for errorProp in this.$detailsEl.find("[data-error-attr]"))						
					errorProp.visualiseRules() 

			toggle: () -> 
				error = this
				$error = this.$errorEl
				$details = this.$detailsEl

				if $error.hasClass('expanded')
					$error.removeClass('expanded')
					$error.addClass('collapsed')
				else		
					$error.removeClass('collapsed')
					$error.addClass('expanded')	
					
					if Errordite.ruleManager? and not $error.data 'rules-visualised'
						openedErrors.push this
						$error.data 'rules-visualised', true		
						
						$details.find('[data-error-attr]').each ->	
							$errorAttr = $ this
							propVal = $errorAttr.text()

							return if propVal.trim? and not propVal.trim()

							$buttons = $('<span class="rule-controls hide"/>')																

							$button = $('<button/>')
								.addClass('btn')
								.addClass('btn-rule')
								.addClass('make-rule') 
								.text('Create Rule')						
									
							$removeButton = $('<button/>')
								.addClass('btn')
								.addClass('btn-rule')
								.addClass('remove-rule') 
								.text('Remove Rule')						
								.hide()

							$buttons.append $button, $removeButton
							
							$errorAttr.on 'mouseenter', () -> 
								$buttons.removeClass 'hide'
																	
							$errorAttr.on 'mouseleave', () -> 
								$buttons.addClass 'hide'	
								$errorAttr.unbind 'mousemove'						

							$textSpan =  $('<span/>').addClass('prop-val').text(propVal)
							$errorAttr.html $textSpan
							$errorAttr.append $buttons
										
							if $errorAttr.data('error-attr') == 'StackTrace'	
								$buttons.css
									position: 'absolute'
								$errorAttr.on 'click', (e) -> 
									$buttons.css 
										top: e.offsetY
										left: e.offsetX		

							getRule = () -> 
								rule = new Errordite.Rule()
								rule.prop = $errorAttr.data 'error-attr'
								###
								IE8 and lower do not support window.getSelection.  Tried using IERange (google it) to fill in the
								gaps but couldn't be bothered to get it to work properly. Rainy day job (or not at all).
								###

								if not window.getSelection?
									rule.op = 'Equals'
									rule.val = propVal
								else
									selection = window.getSelection()
									selectedRange = selection.getRangeAt(0)
									propValSpan = $errorAttr.find '.prop-val'
									###
									Because of the messing about with the text we do in terms of inserting "rule-match" spans
									we can't just use the whole contents as the text range for comparison; instead we need
									to separately get the 1st and last.
									###
									startTextRange = document.createRange()
									startTextRange.selectNodeContents propValSpan.contents()[0]

									endTextRange = document.createRange()
									endTextRange.selectNodeContents propValSpan.contents().last()[0]

									rangeComparison = 
										start: selectedRange.compareBoundaryPoints(Range.START_TO_START, startTextRange)
										end: selectedRange.compareBoundaryPoints(Range.END_TO_END, endTextRange) 

									if selection.toString() == '' or rangeComparison.start < 0 or rangeComparison.end > 0
										rule.val = propVal
										rule.op = 'Equals'
									else
										rule.val = selectedRange.toString()
										if rangeComparison.start == 0 
											if rangeComparison.end == 0
												rule.op = 'Equals'
											else
												rule.op = 'StartsWith'
										else
											if rangeComparison.end == 0
												rule.op = 'EndsWith'
											else
												rule.op = 'Contains'
									rule

							$button.on 'mouseenter', () -> 
								rule = getRule()
								$this = $ this
								$this.attr 'title', "Click to add rule: '#{rule.description()}'"

							$button.on 'click', (e) ->							
								rule = getRule()

								newRule = Errordite.ruleManager.addRule rule.prop,
									rule.op,					
									rule.val

								errorProp = new ErrorProp ($button.closest('[data-error-attr]'))
								errorProp.visualiseRules()
								e.stopPropagation()
						
						this.visualiseRules()

				$details.toggle()

				

				

				