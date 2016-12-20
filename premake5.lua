workspace "blojob"
	configurations { "Debug", "Release" }
	targetdir "bin/%{cfg.buildcfg}"
	startproject "blojob-view"
	
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
		entrypoint "arookas.blojobView"
		
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
		
	project "blojob-view"
		kind "ConsoleApp"
		language "C#"
		namespace "arookas"
		location "blojob-view"
		
		links { "arookas", "blojob", "OpenTK", "System", "System.Drawing" }
		
		files {
			"blojob-view/**.cs",
		}
		
		excludes {
			"blojob-view/bin/**",
			"blojob-view/obj/**",
		}
