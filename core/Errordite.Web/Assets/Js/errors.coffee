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
			$('body').on 'changedrule', (e, rule) -> 
				error.visualiseRules() for error in openedErrors
			$('body').on 'ruleadded', ->
				error.visualiseRules() for error in openedErrors	

		$('body').on 'remove', 'tr.rule', (e) ->
			$tr = $ this
			id = $tr.data 'counter'
			#even if removing the rule means that the visualisation can now be better, it be made better because we're just
			#dealing with the rule that was removed...
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
				this.$propEl.delegate '.new-rule-match, .rule-match', 'click', (e) =>				
					$('.last-selected').removeClass 'last-selected'
					$('.remove-rule').hide()
					$ruleMatch = $ e.currentTarget
					this.selectRule $ruleMatch.data('rule-id')
					e.doNotUnselect = true

			selectRule: (ruleId) -> 
				$ruleMatches = this.$propEl.find('.new-rule-match, .rule-match').filter("[data-rule-id=#{ruleId}]")
				$ruleMatches.addClass 'last-selected'
				#$ruleMatch.addClass 'last-selected'
				this.$propEl.find('.remove-rule')	
					.show()
					.unbind('click')
					.bind 'click', (e) -> 
						Errordite.ruleManager.removeRule ruleId
						$(this).closest('.rule-controls').addClass 'hide'
						$(this).hide()
						e.stopPropagation()
					.attr('title', "Click to remove Rule: '#{$ruleMatches.attr('title')}'")	

			visualiseRules: ->
				return null if not this.propName
				
				#get every match info for every rule works on this property
				matchInfos = _.flatten (this.getMatchInfos rule for rule in Errordite.ruleManager.rules when rule.prop == this.propName)
				#sort by position, working backwards (this means we can modify the string starting at the right 
				#without affecting the position required for subsequent (i.e. "earlier") matches)
				matchInfos = _.sortBy matchInfos, (matchInfo) -> -matchInfo.start
				
				#todo:filter out overlaps

				propValText = this.$propEl.find('.prop-val').text()

				prevMatchInfo = null								

				visualisedHtml = if matchInfos.length == 0 then _.escape propValText else propValText
				 
				i = 0
				for matchInfo in matchInfos		
					
					length = matchInfo.length
					gapToPrev = if prevMatchInfo? then prevMatchInfo.start - matchInfo.end else visualisedHtml.length - matchInfo.end
					
					#see http://stackoverflow.com/questions/1068280/javascript-regex-multiline-flag-doesnt-work
					#[\S\s] is a hack to allow us to match newline characters (class of characters or its negation)
					regex = ///^([\S\s]{#{matchInfo.start}})([\S\s]{#{length}})([\S\s]{#{gapToPrev}})([\S\s]*)///	

					#we need to make sure we only escape each bit of the text once, so divide into chunks and process
					#only works if we've ensured there are no overlaps
					visualisedHtml = visualisedHtml.replace(regex, (m, beforeMatch, matchedBit, gapToPrev, prevAndAfter, offset) ->
						first = i == 0 #i.e. first processed - last if reading left-to-right
						last = ++i == matchInfos.length #vice versa
						"""
						#{if last then _.escape beforeMatch else beforeMatch}<span data-rule-id='#{matchInfo.rule.counter}' 
						class='ruletip #{if matchInfo.rule.status == 'new' then 'new-' else ''}rule-match' 
						title='#{matchInfo.rule.description()}'>#{_.escape(matchedBit)}</span>#{_.escape gapToPrev}#{prevAndAfter}
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

			getControls: (isMultiLine) ->
				###
				The result of this is like <div class=rule-controls><div class=buttons>buttons</div><div>&nbsp;</div></div>
				The purpose of the div with just a space is to allow us to have some space between the cursor and the controls
				panel but without having the mouseleave event trigger if we are at the top of the stack trace area.s
				###
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

				$buttons = $('<div/>')
					.addClass('rule-controls')
					.addClass('hide')													
					.append $('<div/>').addClass('buttons').append($button, $removeButton)

				$buttons.append($('<div/>').html('&nbsp;')) if isMultiLine

				ret =
					#notice indent, which returns a complex object.  CS is strange sometimes!s
					$button: $button
					$removeButton: $removeButton
					$buttons: $buttons

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

							isMultiLine = $errorAttr.data('error-attr') == 'StackTrace'

							propVal = $errorAttr.text()

							return if propVal.trim? and not propVal.trim()

							controls = error.getControls isMultiLine
							$button = controls.$button
							$buttons = controls.$buttons
							

							$errorAttr.on 'mouseenter', () -> 
								$buttons.removeClass 'hide' if !isMultiLine
																	
							$errorAttr.on 'mouseleave', () -> 
								$buttons.addClass 'hide'
								$errorAttr.unbind 'mousemove'						

							$textSpan =  $('<span/>').addClass('prop-val').text(propVal)
							$errorAttr.html $textSpan
							$errorAttr.append $buttons
										
							$errorAttr.on 'click', (e) -> 
								if !e.doNotUnselect
									$('.last-selected').removeClass 'last-selected'

							if isMultiLine	
								$buttons.css
									position: 'absolute'
								$errorAttr.on 'click', (e) -> 
									addOffset e
									$buttons.removeClass 'hide'
									$buttons.addClass 'floating'
									$buttons.css 
										top: e.offsetY - 35 #need to be careful not to be too far down here as the click on the "buffer" div causes odd behaviour reappears in top left as "offset" is much smaller.  Must be a nice way to fix this but doesn't immediately leap to mind!
										left: e.offsetX - 48		
							else
								$buttons.addClass 'inline'

							#firefox doesn't have offset coords on the event so we use this to put them in 
							addOffset = (event) ->
								element = event.currentTarget
								if !event.offsetX
									event.offsetX = (event.pageX - $(element).offset().left)
									event.offsetY = (event.pageY - $(element).offset().top)

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
								errorProp.selectRule newRule.counter

								#for some reason, IE ends up with the whole of the stack trace selected after rule added
								#so explicitly clear it all here
								document.selection.empty() if document.selection
								
						this.visualiseRules()

				$details.toggle()

				

				

				