!macro UMUI_BG
		SetOutPath "$PLUGINSDIR"
		File "${NSISDIR}\Contrib\UltraModernUI\BGSkins\brown\BackGround2.bmp"
		BgImage::SetBg /FILLSCREEN "$PLUGINSDIR\BackGround2.bmp"
		CreateFont $1 "Verdana" 30 700
		BgImage::AddText "$(^Name)" $1 131  85  43 16 114 -1 -1
		BgImage::AddText "$(^Name)" $1 255 255 255 12 110 -1 -1
		BgImage::Redraw
!macroend
!macro UMUI_BG_Destroy
		BgImage::Destroy
!macroend
