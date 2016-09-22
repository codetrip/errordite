
var root = '../../core/Errordite.Web/Assets/';

sassFiles = {};
sassFiles[root + 'css/errordite.css'] = root + 'css/errordite.scss';

cssFiles = {};
cssFiles[root + 'css/errordite.css'] = root + 'css/errordite.css';

coffeeFiles = {};
coffeeFiles[root + 'Js']

module.exports = function(grunt) {
	grunt.initConfig({
		pkg: grunt.file.readJSON('package.json'),
		sass: {
		    dist: {
		        options: {
		            style: 'expanded'
		        },
		        files: sassFiles
		    }
		},
		watch: {
		    sass: {
		        files: [root + 'css/**/*.scss'],
		        tasks: ['sass', 'postcss'],
		        options: {
		            spawn: false
		        }
		    },
            coffee: {
		      files: [root + '**/*.coffee'],
		      tasks: ['newer:coffee'],
		      options: {
		        spawn: false,
		      }
		    }
		},
		focus: {
		
		},
        coffee: {
            glob_to_multiple: {
                expand: true,
                flatten: false,
                cwd: root,
                src: [                 	
                	root + '/**/*.coffee'                	
            	],
                dest: root,
                ext: '.js',
                extDot: 'last'
              }
        },
        postcss: {
        	options: {
        		map: true,
        		processors: [
        			require('autoprefixer')({browsers: ['last 4 versions', 'ie 8', 'ie 9']})
        		]
        	},
        	dist: {
		        files: cssFiles
		    }
        },		
		cssmin: {
		    dist: {
		        files: cssFiles
		    }
		}
	});

	grunt.loadNpmTasks('grunt-browser-sync');
	grunt.loadNpmTasks('grunt-contrib-sass');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-postcss');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks('grunt-contrib-coffee');
    grunt.loadNpmTasks('grunt-newer');
    grunt.loadNpmTasks('grunt-string-replace');
    grunt.loadNpmTasks('grunt-focus');

	grunt.registerTask('dev', ['newer:coffee','sass', 'postcss', 'watch']);
}