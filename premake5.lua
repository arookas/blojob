
workspace "blojob"
	configurations { "Debug", "Release" }
	targetdir "bin/%{cfg.buildcfg}"
	startproject "pablo"
	
	filter "configurations:Debug"
		defines { "DEBUG" }
		flags { "Symbols" }
	
	filter "configurations:Release"
		defines { "RELEASE" }
		optimize "On"
	
	project "blojob"
		kind "SharedLib"
		language "C#"
		namespace "arookas"
		location "blojob"
		
		links { "arookas", "OpenTK", "System", "System.Drawing" }
		
		files {
			"blojob/**.cs",
			"blojob/shader/**.vp",
			"blojob/shader/**.fp",
		}
		
		excludes {
			"blojob/bin/**",
			"blojob/obj/**",
		}
		
		filter "files:**.vp"
			buildaction "Copy"
		
		filter "files:**.fp"
			buildaction "Copy"
		
	project "pablo"
		kind "ConsoleApp"
		language "C#"
		namespace "arookas"
		location "blojob-view"
		entrypoint "arookas.pablo"
		targetname "pablo"
		
		links { "arookas", "blojob", "OpenTK", "System", "System.Drawing" }
		
		files {
			"blojob-view/**.cs",
		}
		
		excludes {
			"blojob-view/bin/**",
			"blojob-view/obj/**",
		}
		
	project "joblo"
		kind "ConsoleApp"
		language "C#"
		namespace "arookas"
		location "blojob-conv"
		entrypoint "arookas.joblo"
		targetname "joblo"
		
		links { "arookas", "blojob", "System" }
		
		files {
			"blojob-conv/**.cs",
		}
		
		excludes {
			"blojob-conv/bin/**",
			"blojob-conv/obj/**",
		}
