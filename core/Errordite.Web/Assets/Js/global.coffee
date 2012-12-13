window.Errordite = {}

`RegExp.escape= function(s) {
	return s.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&');
};`

#TODO: put these in Errordite namespace

class Initialisation

	init: (ajax, pagingFunc) -> 		
		#todo - need to distinguish between things to be initialised after an ajax call and those to be globally initialised for the page
		$('.icon-info').tooltip()
		$('.tool-tip').tooltip()
		$('div.search-box').tooltip()
		$('.dropdown-toggle').dropdown()
		$paging = $('div.paging')	
		#if $paging.length > 0
		paging = new Paging(pagingFunc) 
		paging.init ajax

		$tabHolders = $('.tabs')

		prettyPrint();		

		for tabHolder in $tabHolders			
			controller = new Tabs(tabHolder)
			#GT: we do not actually use the fact that we are storing the controller against the tab holder
			#but it seems if we're going to create a class we should put it somewhere accessible
			$(tabHolder).data 'controller', controller
			controller.init()
		
		Errordite.Spinner.enable()

		#GT: this code isn't actually used yet, so feel free to modify.  Just planning
		#some default ajax code that can handle simple situations.
		$('body').on 'click', 'a.ajax', (e) -> 
			e.preventDefault()
			$.ajax 
				url: this.href
				type: if $(this).hasClass('ajax-post') then 'post' else 'get'
				success: (data) ->
					alert data
				failure: ->
					'failed'
		$('body').on 'click', '[data-confirm]', ->
			confirm $(this).data('confirm')

	datepicker: ($root) ->
		$root.find('div#daterange').daterangepicker
			ranges:
				Today: ["today", "today"]
				Yesterday: ["yesterday", "yesterday"]
				"Last 7 Days": [Date.today().add(days: -6), "today"]
				"Last 30 Days": [Date.today().add(days: -29), "today"]
				"This Month": [Date.today().moveToFirstDayOfMonth(), Date.today().moveToLastDayOfMonth()]
			, (start, end) ->
				$('#daterange span').html start.toString('MMMM d, yyyy') + ' - ' + end.toString('MMMM d, yyyy')
				$('#daterange input').val start.toString('u') + '|' + end.toString('u')


class Spinner

	disable: () -> 
		$('.spinner').ajaxStart(() -> $(this).hide()).ajaxStop(() -> $(this).hide())
	enable: () ->
		$('.spinner').ajaxStart(() -> $(this).show()).ajaxStop(() -> $(this).hide())

###
The idea with Tabs is as follows:
 1. each set of tabs headers (identified by a container having the class "tabs") gets initialised with a Tab Manager (instance of "Tabs" class)
 2. the corresponding tab bodies appear somewhere on the page and have ids that correspond to the data-val attribute value of their headers
 3. changing a tab pushes state to the history (this should probably be parameterised - true/false)
 4. if something needs to happen when a tab is shown, you can bind to the "Shown" event on the .tablink element inside the tab header (example in issues.coffee)
 5. to get to a particular Tab Manager, call Tabs.get(), passing any node inside the .tabs element (or the .tabs element itself)

It could do with a little tweaking and polishing to make some of the names line up better and have fewer significant elements but the principle is
that the tabs get initialised and then we use events for anything instance-specific.
###
class Tabs	
	
	@get: (anyNodeInside) ->
		$tabHolder = $(anyNodeInside).closest '.tabs'
		return null if not $tabHolder.length
		tabManager = $tabHolder.data 'controller'
		if not tabManager?
			tabManager = new Tabs $tabHolder
			tabManager.init()
			$tabHolder.data 'controller', tabManager
		tabManager

	constructor: (tabHolder) -> 
		this.node = $(tabHolder)
		#the parent node is one that contains both the tab header and the tab content
		this.parentNode = this.node.closest(':has(.tab)')
		
	show : (tabName) -> 
		if this.parentNode.length == 0
			return

		$tab = this.parentNode.find('div#' + tabName)
		return false if not $tab.length 
		inactiveNode = this.node.find('li.active')
		inactiveNode.removeClass('active')
		inactiveNode.addClass('inactive')

		$activeNode = $("li:has(a[data-val=#{tabName}])")
		$activeNode.addClass('active')
		$activeNode.removeClass('inactive')		
		this.parentNode.find('div.tab').addClass('hidden')			
		$tab.removeClass('hidden')

		$activeNode.find('.tablink').trigger 'shown'

	init: () ->
		#we want to make sure we don't init any given tab holder multiple times (previously 
		#clicking on errors tab would multi-init and we'd get the popstate firing multiple times)
		if this.node.data('init') == true
			return
					
		this.node.data 'init', true

		if this.parentNode.length == 0
			return
					
		first = true

		#GT: this popstate stuff is pretty close to being correct now I think.  Please don't change it drastically without a lot of caution!

		window.onpopstate = (evt) =>				
			#we don't want to handle the popstate that fires on first entering a page - as the server will have put us on the right tab			
			if first 
				first = false
			else	
				#if there is a state, show that tab; if not, show the first tab
				this.show evt.state || this.node.find('li a [data-val]:first').data 'val'

		
		this.node.delegate 'li a.tablink', 'click', (e) => 
			e.preventDefault()
			$a = $ e.currentTarget
			tabName = $a.data 'val'
			this.show tabName

			if not window.history.pushState?
				return

			window.history.pushState tabName, '', $a.attr 'href'

###
Paging is the class responsible for all Paging operations.  It's a bit neither-one-thing nor another
it is a singleton but of course there could be multiple paging controls on the page.  This means whenever you 
do something you have to specify which $paging div you are talking about.  A potential improvement could be
to instantiate a Paging class each time you do something, telling it at this time which one you are talking about.
###
class Paging

	constructor: (changeFunc) -> 
		paging = this
		this.currentPage = 0
		this.currentSize = 0
		this.changeFunc = changeFunc
		this.pushState	 = false
		this.rootNode = $('body')
		this.baseUrl = this.rootNode.find('input#page-link').val();
		this.contentNode = $('div.content')

		###
		Once we've worked out what url we want to navigte to we call navigate.  $paging is the .paging div
		that holds our paging controls.
		###
		this.navigate = ($paging, url) ->
			
			$ajaxContainer = $paging.closest '.ajax-container'
			if $ajaxContainer.length
				$.get url, {}, (data) ->
					$ajaxContainer.html data
			else
				window.location.href = url	
		
		this.getBaseUrl = ($paging) ->
			$paging.find('input#page-link').val()			 

		this.init = -> 

			this.rootNode.delegate 'input#pgno', 'blur', (e) -> 
				e.preventDefault()
				$this = $ this
				$paging = $this.closest '.paging'
				if $this.val() != $this.data 'currentPage'
					paging.navigate $paging, decodeURI(paging.getBaseUrl($paging).replace('[PGNO]', $this.val()).replace('[PGSZ]', $paging.find('select#pgsz').val()))				
		
			this.rootNode.delegate 'input#pgno', 'focus', (e) -> 
				e.preventDefault()
				$this = $ this
				$this.data 'currentPage', $this.val()					
		
			this.rootNode.delegate 'select#pgsz', 'change', (e) -> 
				e.preventDefault()
				$this = $ this
				$paging = $this.closest '.paging' 
				#need to find current page size and number and work out new page number...				
				firstItemNumber = ($paging.find('input#pgno').val() - 1) * $this.data('current') + 1
				newPageNumber = Math.floor((firstItemNumber / $this.val())) + 1;
				paging.navigate $paging, decodeURI(paging.getBaseUrl($paging).replace('[PGNO]', newPageNumber).replace '[PGSZ]', $this.val())
		
			this.rootNode.delegate 'div.pagination a', 'click', (e) -> 
				e.preventDefault();
				$this = $ this
				$paging = $this.closest '.paging'
				if $this.hasClass('active') || $this.hasClass('disabled')
					return

				paging.navigate $paging, $this.attr('href') 				

			this.contentNode.delegate 'th.sort a', 'click', (e) -> 
				e.preventDefault();
				$this = $ this
				$paging = $this.closest '.paging'
				paging.navigate $paging, $this.attr('href') 				
	
window.Tabs = Tabs
window.Paging = Paging
window.Initalisation = Initialisation

window.Errordite.Spinner = new Spinner();

jQuery -> 
	init = new Initialisation()
	init.init(false)