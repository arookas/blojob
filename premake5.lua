workspace "blojob"
	configurations { "Debug", "Release" }
	targetdir "bin/%{cfg.buildcfg}"
	
	filter "configurations:Debug"
		defines { "DEBUG" }
		flags { "Symbols" }
	
	filter "configurations:Release"
		defines { "RELEASE" }
		optimize "On"
	
	project "blojob"
		kind "ConsoleApp"
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
