
Global Preserve Integer Data(20, 5), Pallet_Count, PCB_Count, Pallet_Count_Convert, Count_Vacc_Ss, remember_odd_place, Place_Odd_point, NG_Product_Count, Ok_Product_Count
Global Preserve Real t1
Integer i, sp, sps, Model, Select_T, Pallet_Pick_Number_In, Pick_Number_In, Pallet_Place_Number_In, Place_Number_In, Pallet_Place_Tool_In, NG_Pos_Num_In, Pos_NG_Num, Pallet_Pick_Number_In_New
Global Preserve Integer BypassRotate, rowPalletpick, columnPalletpick, rowPalletPlace, columnPalletPlace, NumberHandOnTool, NumberPalletPick
Global Preserve Boolean Inpos_PCB_Pick, Inpos_PCB_Place, PLC_Send_Pallet_Data_Done
Global Preserve Integer Pos_Convert(250), Data_Pos_PLC_OK(250)
Global Preserve Integer Count_Len_StrInput
Function main
'==================================================================
'Position Error Data 
	'FineDist 0.1
'==================================================================
'Tool Weight Data 
 	Weight 3
 	'Inertia 0.042
 	Inertia 0.01
'==================================================================
'Torque Data 
 	'LimitTorque 100, 100, 100, 100
'==================================================================
    Off At_Home
	Call Model_Input
    Power High
	Call SIG
	Call R_Speed
	Call Speeds_Pick_Up
	Call Pallet_Data

	Do
		
		If Sw(Go_Home) = On Then
			Call Home_Move
		ElseIf Sw(Pick_Panel_Wait) = On And Oport(Pick_Panel_Wait_InPos) = Off Then
			Call Pick_Panel_Wait_Move
		ElseIf Sw(Pick_Panel) = On And Oport(Pick_Panel_InPos) = Off Then
			Call Pick_Panel_Move
		ElseIf Sw(Place_Panel_Wait) = On And Oport(Place_Panel_Wait_InPos) = Off Then
			Call Place_Panel_Wait_Move
		ElseIf Sw(Place_Panel) = On And Oport(Place_Panel_InPos) = Off Then
			Call Place_Panel_Move
		ElseIf Sw(PCB_Pick_Wait) = On And Oport(PCB_Pick_Wait_InPos) = Off Then
			Call PCB_Pick_Wait_Move
		ElseIf Sw(PCB_Pick) = On And Oport(PCB_Pick_InPos) = Off Then
			Call PCB_Pick_Move
		ElseIf Sw(PCB_Pick_InPos_Re) = On And Oport(PCB_Pick_Com) = Off Then
		    Call Up_PCB_Pick_Move
		ElseIf Sw(PCB_Place_Wait) = On And Oport(PCB_Place_Wait_InPos) = Off Then
			Call PCB_Place_Wait_Move
		ElseIf Sw(PCB_Place) = On And Oport(PCB_Place_InPos) = Off Then
			Call PCB_Place_Move
		ElseIf Sw(Place_NG) = On And Oport(Place_NG_Inpos) = Off Then
			Call PCB_NG_Move
		ElseIf Sw(PLC_Send_Pallet_Convert) = On And PLC_Send_Pallet_Data_Done = False Then
		    Call PLC_Send_Pallet_Convert_Data
		EndIf
		
		Wait 0.03
	Loop

Fend
Function Home_Move
	Print "Home_Move"
	'Power Low
	Call Low_Speed
Up_Z:
	If CZ(Here) > -10 Then
		Move Here :Z(-11)
	EndIf
    If CZ(Here) > -10 Then
    	GoTo Up_Z
    EndIf
	If InsideBox(12, 1) = True Then
'		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		Jump Homp C0 LimZ -10; Off Out_Robot_2_Interlock
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
	    Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP
		Jump Homp; Off Out_Robot_2_Interlock
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		'Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
        Move PCB_Pick_Wait_Pos :Z(-10) CP
		Jump Homp C0 LimZ -10; Off Out_Robot_2_Interlock
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		'Jump PCB_Place_Wait_Pos C0 LimZ -10 CP
		Move PCB_Place_Wait_Pos :Z(-10) CP
		Jump Homp C0 LimZ -10; Off Out_Robot_2_Interlock

	Else
		Jump Homp; Off Out_Robot_2_Interlock

	EndIf
	
    If CZ(Here) > -10 Then
    	GoTo Up_Z
    EndIf
    
	On At_Home, 2;

		
	Power High

Fend
Function SIG
    Wait Oport(0) = On Or Oport(1)
	    On Auto_Mode
	    Off At_Home
	PLC_Send_Pallet_Data_Done = False;
	Off Out_Robot_2_Interlock; Off Robot_Busy; Out 65, 0; Out 66, 0; Out 67, 0; Out 68, 0; Out 69, 0; Out 70, 0; Out 71, 0; BypassRotate = 0; Off PCB_Place_InPos; Off PCB_Place_Re; Off PCB_Place_Com;
Do
	If Motor = Off Then
		Wait 1
		Motor On
		GoTo nextt
	ElseIf Motor = On Then
		GoTo Nextt
	EndIf
Loop

Nextt:

	Power High

Fend
Function Pick_Panel_Wait_Move
	
	Print "Pick_Panel_Wait Move !"
	
	If TaskState(Check_Inposition) = 0 Then
			Call Check_Inposition
	EndIf
	If Out(Pos_Inpos) = 2 Then
		GoTo End_Pick_Panel_Wait
	EndIf
		
	Call Low_Speed
	On Robot_Busy; On Out_Robot_2_Interlock; Wait Sw(In_Robot_1_Interlock) = Off
'	On Pick_Panel_Wait_Re, 0.1
		
	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Pick_Panel_Wait_Pos CP

	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		Jump PCB_Place_Wait_Pos CP
		Jump PCB_Pick_Wait_Pos CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP

	Else
		Jump PCB_Pick_Wait_Pos CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP

	EndIf

	Jump Pick_Panel_Wait_Pos
End_Pick_Panel_Wait:
    If TaskState(On_Pick_Panel_Wait_InPos) = 0 Then
			Xqt On_Pick_Panel_Wait_Inpos
    EndIf
	'On Pick_Panel_Wait_InPos, 0.1
	Off Robot_Busy
	
Fend
Function Pick_Panel_Move

	Print "Pick_Panel Move !"
	
	
	If TaskState(Check_Inposition) = 0 Then
			Call Check_Inposition
	EndIf
	If Out(Pos_Inpos) = 3 Then
		GoTo End_Pick_Panel
	EndIf
		
	
	Call Low_Speed
	On Robot_Busy; On Out_Robot_2_Interlock; Wait Sw(In_Robot_1_Interlock) = Off
	
	'On Pick_Panel_Re, 0.1
	
	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Pick_Panel_Wait_Pos CP
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
'		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		Jump PCB_Place_Wait_Pos CP
		Jump PCB_Pick_Wait_Pos CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP

	Else
		Jump PCB_Pick_Wait_Pos CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP

	EndIf

	
	Jump Pick_Panel_Pos
End_Pick_Panel:
    If TaskState(On_Pick_Panel_InPos) = 0 Then
			Xqt On_Pick_Panel_Inpos
    EndIf
	Print Here
	'On Pick_Panel_InPos, 0.1
	Off Robot_Busy
	
Fend
Function Place_Panel_Wait_Move
	
	Print "Place_Panel_Wait Move !"
	
	If TaskState(Check_Inposition) = 0 Then
			Call Check_Inposition
	EndIf
	If Out(Pos_Inpos) = 4 Then
		GoTo End_Place_Panel_Wait
	EndIf
			
	Call Low_Speed
	On Robot_Busy;
	'On Place_Panel_Wait_Re, 0.1

	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter CP
		Jump PCB_Pick_Wait_Pos CP
		Off Out_Robot_2_Interlock
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		Jump Place_Panel_Wait_Pos C0 LimZ -10 CP

	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		Jump PCB_Place_Wait_Pos C0 LimZ -10 CP

	EndIf

	Jump Place_Panel_Wait_Pos C0 LimZ -10
End_Place_Panel_Wait:
    If TaskState(On_Place_Panel_Wait_InPos) = 0 Then
			Xqt On_Place_Panel_Wait_Inpos
    EndIf

	'On Place_Panel_Wait_InPos, 0.1
	Off Robot_Busy;
	
Fend
Function Place_Panel_Move
	
	Print "Place_Panel Move !"
	
	If TaskState(Check_Inposition) = 0 Then
			Call Check_Inposition
	EndIf
	If Out(Pos_Inpos) = 5 Then
		GoTo End_Place_Panel
	EndIf
	
	Call Low_Speed
	On Robot_Busy;
'	On Place_Panel_Re, 0.1
	
	
	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		Off Out_Robot_2_Interlock

		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		Jump Place_Panel_Wait_Pos C0 LimZ -10 CP
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		Jump PCB_Place_Wait_Pos C0 LimZ -10 CP
	'	Jump Place_Panel_Wait_Pos C0 LimZ -10 CP
	EndIf

	Jump Place_Panel_Pos C0 LimZ -10
	
	Wait 0.1
	'Jump Place_Panel_Wait_Pos C0 LimZ -10 CP
End_Place_Panel:
    If TaskState(On_Place_Panel_InPos) = 0 Then
			Xqt On_Place_Panel_Inpos
    EndIf
	'On Place_Panel_InPos, 0.1
	Off Robot_Busy;
	
Fend
Function Remove_Recycle_Wait_Move
	
	Print "Place_Panel Move !"
	Call Low_Speed
	On Robot_Busy;
	On Place_Panel_Re, 0.1

	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		Off Out_Robot_2_Interlock
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		Jump Place_Panel_Wait_Pos C0 LimZ -10 CP
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		Jump PCB_Place_Wait_Pos C0 LimZ -10 CP
		Jump Place_Panel_Wait_Pos C0 LimZ -10 CP

	Else
		Jump Place_Panel_Wait_Pos C0 LimZ -10 CP

	EndIf

	Jump Place_Panel_Pos C0 LimZ -10

	Jump Place_Panel_Pos :Z(0) C0 LimZ -10
		

	
	
Fend
Function Remove_Recycle_Move
	Off Robot_Wait_Function;

	Print "Remove_Recycle Move !"
	Call Low_Speed
	On Robot_Busy; On Out_Robot_2_Interlock; Wait Sw(In_Robot_1_Interlock) = Off

	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
'		Jump PCB_Pick_Wait_Pos :z(0) C0 LimZ -10 CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		Jump PCB_Place_Wait_Pos :Z(0) C0 LimZ -10 CP
		Jump PCB_Pick_Wait_Pos :Z(0) C0 LimZ -10 CP
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP

	Else
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP

	EndIf

	Wait Sw(In_Robot_1_Interlock) = Off

	Jump Remove_Recycle_Pos C0 LimZ -10


	
Fend
Function PCB_Pick_Wait_Move
	Pallet_Pick_Number_In = In(Pallet_Pick_Number);
	
	
	If TaskState(Check_Inposition) = 0 Then
			Call Check_Inposition
	EndIf
	If Out(Pos_Inpos) = 6 Then
		GoTo Pick_End
	EndIf
	
    	' add 24-03-25 improve trouble drop ea when pick up
	If (CX(Here) <= (CX(PCB_Pick_Wait_Pos) + 12)) Or CX(Here) >= (CX(PCB_Pick_Wait_Pos) - 10) Then
	    If (CZ(Here) <= (CZ(PCB_Pick_1_1) + 2)) Or CZ(Here) <= (CZ(PCB_Pick_1_1) - 2) Then
	        Xqt Low_Speed
	        'Wait 0.1
	        Move Here :Z(-54)
        EndIf
	EndIf
	
	' add 24-03-25
	On Robot_Busy;
	'On PCB_Pick_Wait_Re, 1
	Print "PCB_Pick_Wait Move !"
	Call R_Speed
	
	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		If (Pallet_Pick_Number_In = 1) Then
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		EndIf
		If (Pallet_Pick_Number_In = 2) Then
		Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		EndIf
		Off Out_Robot_2_Interlock;
	 GoTo Pick_End

	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		If (Pallet_Pick_Number_In = 1) Then
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		EndIf
		If (Pallet_Pick_Number_In = 2) Then
		Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		EndIf
	  GoTo Pick_End
	
	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		'Jump PCB_Place_Wait_Pos C0 LimZ -10 CP
		Move PCB_Place_Wait_Pos :Z(-10) CP
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		If (Pallet_Pick_Number_In = 1) Then
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		EndIf
		If (Pallet_Pick_Number_In = 2) Then
		Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		EndIf
	 GoTo Pick_End

	EndIf
	

	'Jump PCB_Pick_Wait_Pos C0 LimZ -10
	    If (Pallet_Pick_Number_In = 1) Then
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		EndIf
		If (Pallet_Pick_Number_In = 2) Then
		Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		EndIf
	
Pick_End:


    If TaskState(On_PCB_Pick_Wait_Inpos) = 0 Then
			Xqt On_PCB_Pick_Wait_Inpos
    EndIf
    
	'On PCB_Pick_Wait_InPos, 0.1
	Off Robot_Busy; Off Out_Robot_2_Interlock;
	
Fend
Function PCB_Pick_Move
	
	On Robot_Busy
	

	Pallet_Pick_Number_In = In(Pallet_Pick_Number);
	Pick_Number_In = In(Pick_Number);
	Pallet_Pick_Number_In_New = In(Pallet_Pick_Number) - 1;
    Pos_NG_Num = 0
	TmReset (0)
	If TaskState(Check_Inpos_PCB_Pick) = 0 Then
			Call Check_Inpos_PCB_Pick
	EndIf
	If Inpos_PCB_Pick = True Then
		GoTo End_Pick_PCB
	EndIf
	Print "PCB_Pick Move !"
	Call R_Speed
	

		Call PCB_Pick_Check_once
		
	      'add bypass rotate last round
	If ((Pallet_Pick_Number_In = 2) And (Pick_Number_In = 3)) Then
		BypassRotate = 1
		'Print "value bypass rotate", BypassRotate;
	EndIf
	'add bypass rotate last round	
		
	'	Jump Pallet(Pallet_Pick_Number_In, Pick_Number_In) C0 LimZ -10 CP
	 '   Print Pallet(Pallet_Pick_Number_In, Pick_Number_In);
		'Jump Pallet(Pallet_Pick_Number_In, Pick_Number_In) C0 LimZ -10 ! D80; On PCB_Pick_InPos, 0.05 !
'		Jump Pallet(Pallet_Pick_Number_In, Pick_Number_In) C0 LimZ -10 ! D80; On PCB_Pick_InPos !

    If Sw(Bypass_Mode) = On Then
	    Jump Pallet(Pallet_Pick_Number_In_New, Pick_Number_In) :Z(-10)
	EndIf
	If Sw(Bypass_Mode) = Off Then
	    Jump Pallet(Pallet_Pick_Number_In_New, Pick_Number_In) LimZ -10
    EndIf
	Print Pallet(Pallet_Pick_Number_In_New, Pick_Number_In);
	

	
End_Pick_PCB:
    If TaskState(On_PCB_Pick_InPos) = 0 Then
			Xqt On_PCB_Pick_InPos
    EndIf
	Off Robot_Busy; Off Out_Robot_2_Interlock
	
	Print "Pick Motion T/T : ", Tmr(0)

Fend
Function PCB_NG_Move
	
	On Robot_Busy
	On Place_NG_Re, 0.1
	
	Print "PCB_NG_Move ! "
	
	NG_Pos_Num_In = In(NG_Pos_Num)
	Print "NG_Pos_Num_In : ", NG_Pos_Num_In
	If Pos_NG_Num = 1 Then
		GoTo NG_Next
	EndIf
	
	If CX(PCB_Pick_Wait_Pos) >= CX(Here) Then
        Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
	EndIf
	If CX(PCB_Place_Wait_Pos) <= CX(Here) Then

		Jump PCB_Place_Wait_Pos C0 LimZ -10 CP
	EndIf
	
NG_Next:

	If NG_Pos_Num_In = 1 Then
		Jump PCB_NG_Pos_1 C0 LimZ -10
	EndIf
	If NG_Pos_Num_In = 2 Then
		Jump PCB_NG_Pos_2 C0 LimZ -10
	EndIf
	If NG_Pos_Num_In = 3 Then
		Jump PCB_NG_Pos_3 C0 LimZ -10
	EndIf
	If NG_Pos_Num_In = 4 Then
		Jump PCB_NG_Pos_4 C0 LimZ -10
	EndIf
	If NG_Pos_Num_In = 5 Then
		Jump PCB_NG_Pos_5 C0 LimZ -10
	EndIf
	If NG_Pos_Num_In = 6 Then
		Jump PCB_NG_Pos_6 C0 LimZ -10
	EndIf
'	If NG_Pos_Num_In = 7 Then
'		Jump PCB_NG_Pos_7 C0 LimZ -10
'	EndIf
'	If NG_Pos_Num_In = 8 Then
'		Jump PCB_NG_Pos_8 C0 LimZ -10
'	EndIf
'	If NG_Pos_Num_In = 9 Then
'		Jump PCB_NG_Pos_9 C0 LimZ -10
'	EndIf
	
	
	On Place_NG_Inpos, 0.1
	Off Robot_Busy;
	
Fend
Function PCB_Pick_Check_once
	
	Pallet_Pick_Number_In = In(Pallet_Pick_Number);
	
	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		If InsideBox(4, 1) = True Or InsideBox(7, 1) = True Or InsideBox(12, 1) = True Then
			'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
			If (Pallet_Pick_Number_In = 1) Then
		    Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
		    Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf
		Else
			Jump Panel_Enter LimZ -10 CP
			'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
			If (Pallet_Pick_Number_In = 1) Then
		    Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
		    Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf
		EndIf
		
		Off Out_Robot_2_Interlock

	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then

		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    If (Pallet_Pick_Number_In = 1) Then
		    Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
		    Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf

	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		'Jump PCB_Place_Wait_Pos C0 LimZ -10 CP
		Move PCB_Place_Wait_Pos :Z(-10) CP
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    If (Pallet_Pick_Number_In = 1) Then
		    Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
		    Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf

	Else
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    If (Pallet_Pick_Number_In = 1) Then
		    Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
		    Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf

	EndIf

Fend
Function PCB_Place_Wait_Move

    Pallet_Pick_Number_In = In(Pallet_Pick_Number);
    
	If TaskState(Check_Inposition) = 0 Then
			Call Check_Inposition
	EndIf
	If Out(Pos_Inpos) = 9 Then
		GoTo End_Place_PCB_Wait
	EndIf
	Print "PCB Place Wait_ Move"
	On Robot_Busy
'	On PCB_Place_Wait_Re, 0.1

	If CX(PCB_Pick_Wait_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		Off Out_Robot_2_Interlock
		
	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		If InsideBox(5, 1) = True Then
			Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ CZ(PCB_NG_Pos_1) CP
		Else
			If (Pallet_Pick_Number_In = 1) Then
			Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
			Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf
			
			'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		EndIf

	ElseIf CX(PCB_Place_Wait_Pos) - 100 <= CX(Here) Then
		'Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10 CP
		Move PCB_Place_Wait_Pos :Z(-10) CP

	Else
		'Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10 CP
		Move PCB_Place_Wait_Pos :Z(-10) CP

	EndIf


	Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10
End_Place_PCB_Wait:
	If TaskState(On_PCB_Place_Wait_Inpos) = 0 Then
			Xqt On_PCB_Place_Wait_Inpos
	EndIf
		
	'On PCB_Place_Wait_InPos, 0.1
	Off Robot_Busy
	
Fend
Function PCB_Place_Move

	Print "PCB Place Move"
	
	'add 24-10-28
	Pallet_Count = In(Place_Number)
	        If Pallet_Count > 0 And Pallet_Count < 21 Then
			   Call Count_Pallet_Convert
			EndIf
			If Pallet_Count > 20 And Pallet_Count < 41 Then
			   Call Count_Pallet_Convert_2
			EndIf
	'add 24-10-28		
	
	Call R_Speed
	TmReset (1)
	
	Pallet_Place_Tool_In = In(Pallet_Place_Tool) + 1;
	
	'add 24-10-28
	'Place_Number_In = In(Place_Number)
	Place_Number_In = Pallet_Count_Convert
	'add 24-10-28
	
	Pallet_Pick_Number_In = In(Pallet_Pick_Number);
	
	'Check position
	If TaskState(Check_Inpos_PCB_Place) = 0 Then
			Call Check_Inpos_PCB_Place
	EndIf
	If Inpos_PCB_Place = True Then
		GoTo End_PCB_Place_Move
	EndIf
		
	
	On Robot_Busy
	


	If CX(Place_Panel_Pos) - 150 > CX(Here) Then
		Wait Sw(In_Robot_1_Interlock) = Off
		Jump Panel_Enter LimZ -10 CP
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10 CP
		Off Out_Robot_2_Interlock

	ElseIf CX(PCB_Pick_Wait_Pos) - 150 <= CX(Here) And CX(PCB_Place_Wait_Pos) - 100 > CX(Here) Then
		If InsideBox(5, 1) = True Then
			Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ CZ(PCB_NG_Pos_1)
		ElseIf CY(Here) > CY(PCB_Place_Wait_Pos) + 50 Then
			
			Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ CZ(PCB_NG_Pos_1) CP
		Else
			'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
			If (Pallet_Pick_Number_In = 1) And (BypassRotate = 0) Then
		    	Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
		    EndIf
		    If (Pallet_Pick_Number_In = 2) Then
		    	Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
		    EndIf
		    
			Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10 CP

		EndIf
	
	ElseIf CX(PCB_Pick_Wait_Pos) - 100 <= CX(Here) Then
'		If InsideBox(10, 1) = True Then
			GoTo PLT_Check
'		Else
'			Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10 CP
'	
'		EndIf
'	Else
'		Jump PCB_Place_Wait_Pos :Z(-10) C0 LimZ -10 CP
'
	EndIf


PLT_Check:
	'add 24-12-25
	If Place_Number_In = 4 Then
	sp = In(Speed_In)

	Speed sp
	Accel sp * 1.2, sp * 1.2
	SpeedS sp * 20
	AccelS sp * 20, sp * 20
	EndIf
	'add 24-12-25
	'Jump Pallet(Pallet_Place_Tool_In, Place_Number_In) C0 LimZ -10 CP
	'Go Pallet(Pallet_Place_Tool_In, Place_Number_In)
	'Go Pallet(Pallet_Place_Tool_In, Place_Number_In) ! D70; On PCB_Place_InPos, 0.05 !
	'Go Pallet(Pallet_Place_Tool_In, Place_Number_In) ! D70; On PCB_Place_InPos !
	
    If Sw(Bypass_Mode) = On Then
    Go Pallet(Pallet_Place_Tool_In, Place_Number_In) :Z(-10)
    EndIf
    If Sw(Bypass_Mode) = Off Then
    Go Pallet(Pallet_Place_Tool_In, Place_Number_In)
    EndIf

	Print "Pallet_Place_Tool_In", Pallet_Place_Tool_In;
	Print "Place_Number_In", Place_Number_In;
	BypassRotate = 0;
	'On PCB_Place_InPos, 0.1
	
	'add 24-12-25	
	sp = In(Speed_In)

	Speed sp
	Accel sp * 1.2, sp * 1.2
	SpeedS sp * 20
	AccelS sp * 60, sp * 60
	'add 24-12-25

	'On PCB_Place_Com, 0.1; 

End_PCB_Place_Move:
    'If TaskState(On_PCB_Place_InPos) = 0 Then
	'		Xqt On_PCB_Place_InPos
	'EndIf
	On PCB_Place_InPos
	BypassRotate = 0;
	Wait Sw(PCB_Place) = Off
	Off PCB_Place_InPos
	Off Robot_Busy;


Fend
Function Model_Input
	
	Model = In(Model_In)
	LoadPoints "robot" + Str$(Model) + ".pts"
	Print "Current Robot Model No.", Model
	'==================================================================
	'Box Data
		Box 1, CX(Place_Panel_Wait_Pos) + 0.001, 1000, +1000, 1000, +1000, 1000
		Box 2, CX(Pick_Panel_Pos) - 2, CX(Pick_Panel_Pos) + 2, CY(Pick_Panel_Pos) - 2, CY(Pick_Panel_Pos) + 2, CZ(Pick_Panel_Pos) - 2, CZ(Pick_Panel_Pos) + 100
		Box 3, CX(PCB_Pick_2_1) - 5, CX(PCB_Pick_1_1) + 5, CY(PCB_Pick_1_3) - 5, CY(PCB_Pick_2_1) + 5, CZ(PCB_Pick_1_1) - 2, CZ(PCB_Pick_Wait_Pos) + 2
		Box 4, CX(PCB_Place_Wait_Pos) - 2, CX(PCB_Place_Wait_Pos) + 2, CY(PCB_Place_Wait_Pos) - 2, CY(PCB_Place_Wait_Pos) + 2, CZ(PCB_Place_Wait_Pos) - 200, CZ(PCB_Place_Wait_Pos) + 200
		Box 5, CX(PCB_NG_Pos_1) - 20, CX(PCB_NG_Pos_1) + 50, CY(PCB_NG_Pos_1) - 100, CY(PCB_NG_Pos_1) + 100, CZ(PCB_NG_Pos_1) - 200, CZ(PCB_NG_Pos_1) + 200
		
		Box 12, CX(Homp) - 2, CX(Homp) + 2, CY(Homp) - 2, CY(Homp) + 2, CZ(PCB_Pick_1_1) - 2, CZ(Homp) + 2 ' I/O(AT HOME)				
	'==================================================================
	
Fend
Function Pallet_Data
	Integer m;
	m = 0;
	Print "Creat Pallet"
	
    NumberPalletPick = In(Number_Pallet_Pick);
    Print "NumberPalletPick", NumberPalletPick
    
    NumberHandOnTool = In(Number_Tool_On_Robot);
    Print "NumberHandOnTool", NumberHandOnTool
    
    rowPalletpick = In(Number_Row_Creat_Pick_Pallet);
    Print "rowPalletpick", rowPalletpick
    
    columnPalletpick = In(Number_Column_Creat_Pick_Pallet);
    Print "columnPalletpick", columnPalletpick
    
    rowPalletPlace = In(Number_Row_Creat_Place_Pallet);
    Print "rowPalletPlace", rowPalletPlace
    
    columnPalletPlace = In(Number_Column_Creat_Place_Pallet);
    Print "columnPalletPlace", columnPalletPlace
	
'Pick Pallet Data==============================================
	For m = 0 To (NumberPalletPick - 1)
		
		Pallet (m), P(20 + (3 * m)), P(21 + (3 * m)), P(22 + (3 * m)), columnPalletpick, rowPalletpick
		Print "Pallet Pick m + 1 ", m
    Next


	
'Place Pallet Data=============================================
	For m = 0 To (NumberHandOnTool - 1)
		
		Pallet (m + 2), P(30 + (3 * m)), P(31 + (3 * m)), P(32 + (3 * m)), columnPalletPlace, rowPalletPlace
		Print "Pallet Place m + 3 ", m
    Next
    

'==============================================================


Fend

Function R_Speed

	sp = In(Speed_In)

	Speed sp
	Accel sp, sp
	SpeedS sp * 20
	AccelS sp * 60, sp * 60

	Print "Robot Current Speed : ", sp, "%"

Fend
Function Low_Speed
	
	sp = In(Speed_In) * 0.1
	Speed sp
	Accel sp, sp
	SpeedS sp * 1
	AccelS sp * 10, sp * 10
	Print "Robot Current Low_Speed : ", sp, "%"
	
Fend
Function Speeds_Pick_Up

	'sps = In(Speeds_Pick_Up_In)

	'SpeedS sps
	'AccelS sps, sps

	Print "Robot Current Speeds_Pick_Up : ", sps, "mm/s"

Fend
Function On_PCB_Place_Re
	On PCB_Place_Re
	Wait Sw(PCB_Place) = Off
	Off PCB_Place_Re
Fend
Function On_PCB_Place_InPos
	On PCB_Place_InPos, 1
	'Wait Sw(PCB_Place_InPos_Re) = On
	'Off PCB_Place_InPos
Fend
Function On_PCB_Place_Com
	On PCB_Place_Com, 1
	'Wait Sw(Hand_Work_Com) = Off
	'Off PCB_Place_Com
Fend
Function On_PCB_Pick_Re
	On PCB_Pick_Re
	Wait Sw(PCB_Pick) = Off
	Off PCB_Pick_Re
Fend
Function On_PCB_Pick_InPos
	On PCB_Pick_InPos, 1
	'Wait Sw(PCB_Pick_InPos_Re) = On
	'Off PCB_Pick_InPos
Fend
Function On_PCB_Pick_Com
   On PCB_Pick_Com, 1
   'Wait Sw(Hand_Work_Com) = Off
   'Off PCB_Pick_Com
Fend
Function Count_Pallet_Convert
     	' 1
 If Pallet_Count = 1 Then
 	Pallet_Count_Convert = 1
 	' 2
 	ElseIf Pallet_Count = 2 Then
 	Pallet_Count_Convert = 2
 	' 3
 	ElseIf Pallet_Count = 3 Then
 	Pallet_Count_Convert = 3
 	' 4
 	ElseIf Pallet_Count = 4 Then
 	Pallet_Count_Convert = 4
 	' 5
 	ElseIf Pallet_Count = 5 Then
 	Pallet_Count_Convert = 4
 	' 11
 	ElseIf Pallet_Count = 6 Then
 	Pallet_Count_Convert = 3
 	' 12
 	ElseIf Pallet_Count = 7 Then
 	Pallet_Count_Convert = 2
 	' 13
 	ElseIf Pallet_Count = 8 Then
 	Pallet_Count_Convert = 1
 	' 14
 	ElseIf Pallet_Count = 9 Then
 	Pallet_Count_Convert = 1
 	' 15
 	ElseIf Pallet_Count = 10 Then
 	Pallet_Count_Convert = 2
 	' 21
 	ElseIf Pallet_Count = 11 Then
 	Pallet_Count_Convert = 3
 	' 22
 	ElseIf Pallet_Count = 12 Then
 	Pallet_Count_Convert = 4
 	' 23
 	ElseIf Pallet_Count = 13 Then
 	Pallet_Count_Convert = 4
 	' 24
 	ElseIf Pallet_Count = 14 Then
 	Pallet_Count_Convert = 3
 	' 25
 	ElseIf Pallet_Count = 15 Then
 	Pallet_Count_Convert = 2
 	' 11
 	ElseIf Pallet_Count = 16 Then
 	Pallet_Count_Convert = 1
 	' 12
 	ElseIf Pallet_Count = 17 Then
 	Pallet_Count_Convert = 1
 	' 13
 	ElseIf Pallet_Count = 18 Then
 	Pallet_Count_Convert = 2
 	' 14
 	ElseIf Pallet_Count = 19 Then
 	Pallet_Count_Convert = 3
 	' 15
 	ElseIf Pallet_Count = 20 Then
 	Pallet_Count_Convert = 4
 EndIf
 	'Out Pallet_Count_Convert_S, Pallet_Count_Convert
 	'Out remember_odd_place_S, remember_odd_place
 Fend
 Function Count_Pallet_Convert_2
 		' 1
 If Pallet_Count = 21 Then
 	Pallet_Count_Convert = 4
 	' 2
 	ElseIf Pallet_Count = 22 Then
 	Pallet_Count_Convert = 3
 	' 3
 	ElseIf Pallet_Count = 23 Then
 	Pallet_Count_Convert = 2
 	' 4
 	ElseIf Pallet_Count = 24 Then
 	Pallet_Count_Convert = 1
 	' 5
 	ElseIf Pallet_Count = 25 Then
 	Pallet_Count_Convert = 1
 	' 11
 	ElseIf Pallet_Count = 26 Then
 	Pallet_Count_Convert = 2
 	' 12
 	ElseIf Pallet_Count = 27 Then
 	Pallet_Count_Convert = 3
 	' 13
 	ElseIf Pallet_Count = 28 Then
 	Pallet_Count_Convert = 4
 	' 14
 	ElseIf Pallet_Count = 29 Then
 	Pallet_Count_Convert = 4
 	' 15
 	ElseIf Pallet_Count = 30 Then
 	Pallet_Count_Convert = 3
 	' 21
 	ElseIf Pallet_Count = 31 Then
 	Pallet_Count_Convert = 2
 	' 22
 	ElseIf Pallet_Count = 32 Then
 	Pallet_Count_Convert = 1
 	' 23
 	ElseIf Pallet_Count = 33 Then
 	Pallet_Count_Convert = 1
 	' 24
 	ElseIf Pallet_Count = 34 Then
 	Pallet_Count_Convert = 2
 	' 25
 	ElseIf Pallet_Count = 35 Then
 	Pallet_Count_Convert = 3
 	' 11
 	ElseIf Pallet_Count = 36 Then
 	Pallet_Count_Convert = 4
 	' 12
 	ElseIf Pallet_Count = 37 Then
 	Pallet_Count_Convert = 4
 	' 13
 	ElseIf Pallet_Count = 38 Then
 	Pallet_Count_Convert = 3
 	' 14
 	ElseIf Pallet_Count = 39 Then
 	Pallet_Count_Convert = 2
 	' 15
 	ElseIf Pallet_Count = 40 Then
 	Pallet_Count_Convert = 1
 EndIf
 	'Out Pallet_Count_Convert_S, Pallet_Count_Convert
 	'Out remember_odd_place_S, remember_odd_place
 Fend



Function Check_Inpos_PCB_Pick
	Inpos_PCB_Pick = False;
	Print "Check_Inpos_PCB_Pick"
	If (CX(Here) = CX(Pallet(Pallet_Pick_Number_In_New, Pick_Number_In))) And (CY(Here) = CY(Pallet(Pallet_Pick_Number_In_New, Pick_Number_In))) And (CU(Here) = CU(Pallet(Pallet_Pick_Number_In_New, Pick_Number_In))) And (CZ(Here) = CZ(Pallet(Pallet_Pick_Number_In_New, Pick_Number_In))) Then

		        Inpos_PCB_Pick = True;

	Else
		Inpos_PCB_Pick = False;
    EndIf
Fend
Function Check_Inpos_PCB_Place
	Inpos_PCB_Place = False;
	Print "Check_Inpos_PCB_Place"
	If (CX(Here) = CX(Pallet(Pallet_Place_Tool_In, Place_Number_In))) And (CY(Here) = CY(Pallet(Pallet_Place_Tool_In, Place_Number_In))) And (CU(Here) = CU(Pallet(Pallet_Place_Tool_In, Place_Number_In))) And (CZ(Here) = CZ(Pallet(Pallet_Place_Tool_In, Place_Number_In))) Then

		        Inpos_PCB_Place = True;

	Else
		Inpos_PCB_Place = False;
    EndIf
Fend
Function Check_Inposition
	Integer k;
	k = 0;
	Out (Pos_Inpos), 0;
	Print "Check_Inposition"
	For k = 0 To 70
'			      	Out (Pos_Inpos), k + 1;
'            Print "Here now inpos P", k
'                        Print "Send PLC here now inpos P", Out(Pos_Inpos)
	   If (CX(P(k)) <= (CX(Here) + 0.01)) And (CX(P(k)) >= (CX(Here) - 0.01)) And (CY(P(k)) <= (CY(Here) + 0.01)) And (CY(P(k)) >= (CY(Here) - 0.01)) And (CU(P(k)) <= (CU(Here) + 0.01)) And (CU(P(k)) >= (CU(Here) - 0.01)) And (CZ(P(k)) <= (CZ(Here) + 0.01)) And (CZ(P(k)) >= (CZ(Here) - 0.01)) Then

	      	Out (Pos_Inpos), k + 1;
            Print "Here now inpos P", k
            Print "Send PLC here now inpos P", Out(Pos_Inpos)
	   	   	 		 	   	
	   EndIf
	Next
Fend
Function Up_PCB_Pick_Move
	
	'speed low
	sp = In(Speed_In) * 8
	SpeedS sp
	AccelS sp, sp
	
	Move Here :Z(CZ(PCB_Pick_Wait_Pos_Even))
'	Jump Pallet(Pallet_Pick_Number_In, Pick_Number_In) :Z(CZ(PCB_Pick_Wait_Pos_Even)) C0 LimZ -10
	


    If (Pallet_Pick_Number_In = 1) Then
		'Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP ! D20; On PCB_Pick_Com, 0.05 !
'		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP ! D20; On PCB_Pick_Com !
'		Wait Sw(Hand_Work_Com) = Off;
'		Off PCB_Pick_Com
		Jump PCB_Pick_Wait_Pos C0 LimZ -10 CP
	EndIf
	If (Pallet_Pick_Number_In = 2) Then
		'Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP ! D20; On PCB_Pick_Com, 0.05 !
'		Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP ! D20; On PCB_Pick_Com !
'		Wait Sw(Hand_Work_Com) = Off;
'		Off PCB_Pick_Com
		Jump PCB_Pick_Wait_Pos_Even C0 LimZ -10 CP
	EndIf
	
	If TaskState(On_PCB_Pick_Com) = 0 Then
			Xqt On_PCB_Pick_Com
	EndIf
Fend
Function On_PCB_NG_Inpos
   On Place_NG_Inpos, 1
Fend
Function On_PCB_Place_Wait_Inpos
   On PCB_Place_Wait_InPos, 1
Fend
Function On_PCB_Pick_Wait_Inpos
   On PCB_Pick_Wait_InPos, 1
Fend
Function On_Pick_Panel_Inpos
   On Pick_Panel_InPos, 1
Fend
Function On_Pick_Panel_Wait_Inpos
   On Pick_Panel_Wait_InPos, 1
Fend
Function On_Place_Panel_Inpos
   On Place_Panel_InPos, 1
Fend
Function On_Place_Panel_Wait_Inpos
   On Place_Panel_Wait_InPos, 1
Fend
Function Connect(Gate As Integer, IP$ As String, Port As Integer, TimeoutConnect As Integer) As Integer
	Integer resultConnectPLC
	SetNet #Gate, IP$, Port, CRLF, NONE
	OpenNet #Gate As Client
	WaitNet #Gate, TimeoutConnect
	'============
	resultConnectPLC = ChkNet(Gate)
	Connect = resultConnectPLC
Fend
Function ConnectPLC(IDPLC As Integer, ChangeModel As Integer) As Integer
Integer Gate, Port, TimeOutConnect
String IP$
Integer resultConnectPLC
	'============
	If IDPLC = 1 Then
		Gate = 202
		IP$ = "192.168.3.39"
		TimeOutConnect = 5000
    EndIf
    If ChangeModel = 1 Then
		Port = 4096
    EndIf

	resultConnectPLC = Connect(Gate, IP$, Port, TimeOutConnect)
	
	If resultConnectPLC = 0 Then
		Print "PLC", IDPLC, " Connected"
		ConnectPLC = True
	Else
		CloseNet #Gate
		Print "PLC", IDPLC, " Not Connected"
		ConnectPLC = False
		'ErrorPro(g_ErrCodeConnectVision)
	EndIf
	ConnectPLC = Gate
Fend
Function SendCmdPLC(Gate As Integer, Cmd$ As String)
	Print #Gate, Cmd$
	Print "Robot Send Data PLC: ", Cmd$
Fend
Function ReceiveCmdPLC$(Gate As Integer) As String
	String Cmd$
	Integer dataArray(10)
	Print "Gate is:", Gate
    Line Input #Gate, Cmd$
    'ReadBin #Gate, dataArray(), 10
	Print "Robot Received Data PLC: ", Cmd$
	ReceiveCmdPLC$ = Cmd$
Fend

Function PLC_TCP(Gate As Integer)
	String DataReceived_TCP$
		TT1Again:
		SendCmdPLC(Gate, "KyO")
		DataReceived_TCP$ = ReceiveCmdPLC$(Gate)
		Print "PLC: ", DataReceived_TCP$
		Integer lengthStringFromPLC
		lengthStringFromPLC = Len(DataReceived_TCP$)
		' nhan data ok
		If lengthStringFromPLC > 0 Then
		    Print "PLC: TT,1 OK"
		    ' do dai chuoi data doc ve
		    Count_Len_StrInput = Len(DataReceived_TCP$)
	        Print " do dai chuoi nhap vao la", Count_Len_StrInput
	        Integer n, a, m, g, r
              n = 0
              a = 0
              m = 0
              g = 0
              r = 0
	        String String_Check$
	        String String_Slpit$
	        Integer Data_Pos_PLC_Send_OK
	            Data_Pos_PLC_Send_OK = 0
	        Integer Max_Number_Pos_Place_Tray
	        Integer MaxNumperInput
	        MaxNumperInput = In(Number_Row_Creat_Place_Pallet) * In(Number_Column_Creat_Place_Pallet);
	        Print "gia tri place max la ", MaxNumperInput
	        If MaxNumperInput <= 0 Then
	           Error Err_Data_PLC;
	        EndIf
  	        Max_Number_Pos_Place_Tray = In(Number_Pcb_Full_Tray)
	        Print " gia tri Max_Number_Pos_Place_Tray ", Max_Number_Pos_Place_Tray
	        
	       'tach chuoi data
	        For n = 0 To Count_Len_StrInput
	        	   String_Check$ = Mid$(DataReceived_TCP$, n, 1)
		           If String_Check$ = "," Or String_Check$ = ";" Then
		            Print " Finding nemo"
			            String_Slpit$ = Mid$(DataReceived_TCP$, a, (n - a))
			            If Val(String_Slpit$) > 0 And Val(String_Slpit$) <= MaxNumperInput Then
			               Pos_Convert(m) = Val(String_Slpit$)
			               Print "a", Val(String_Slpit$)
			               Print "b", String_Slpit$
			            
			           	   Print "gia tri pos convert la", Pos_Convert(m)
			               m = m + 1
			               Print " phan tu thu ", m
			               Print " co gia tri la ", String_Slpit$
			               a = n + 1
			               g = m - 1
			            EndIf
			            If Val(String_Slpit$) <= 0 Or Val(String_Slpit$) > MaxNumperInput Then
			            	Error Err_Data_PLC
			            	Print " Err data pallet convert from PLC wrong "
			            EndIf
			       EndIf
			       If m > 0 And m <= Max_Number_Pos_Place_Tray Then
			            Data_Pos_PLC_Send_OK = 1
                   EndIf
		           
	        Next
	        ' Check number pcb conver pallet tray 	     
			If m <> Max_Number_Pos_Place_Tray Then
			         Error Err_Data_PLC
			         Print " Err data pallet convert from PLC wrong, number PCB full tray not matching with reality "
            EndIf
	        If Data_Pos_PLC_Send_OK = 1 Then
	            Print " convert OK"
	            For r = 0 To g
	  	        Data_Pos_PLC_OK(r) = Pos_Convert(r)
	  		    Print " phan tu thu ", r
			    Print " co gia tri la ", Data_Pos_PLC_OK(r)
			    Next
			    'Call Convert_Pallet
	        EndIf
		
	    'EndIf
	    ' nhan data fail
		Else
			Print "PLC: TT,1 Fail"
			'GoTo TT1Again
			Wait 1
		EndIf
	' close connect TCP with PLC
	CloseNet #Gate
	Print "Close close connect TCP with PLC"
Fend
Function Convert_Pallet
	Integer n, a
	n = 0;
	a = 0;
	For n = 0 To Count_Len_StrInput
		a = n + 1
		If Pallet_Count = a Then
			Pallet_Count_Convert = Data_Pos_PLC_OK(n)
	  		Print " phan tu thu ", a
			Print " co gia tri la ", Data_Pos_PLC_OK(n)
		EndIf
	Next
	
	
Fend
Function PLC_Send_Pallet_Convert_Data
    Integer Gate
    On Robot_Ch1_Connected_PLC;
	Gate = ConnectPLC(1, 1)
	PLC_TCP(Gate)
	PLC_Send_Pallet_Data_Done = True;
	Off Robot_Ch1_Connected_PLC;
Fend

