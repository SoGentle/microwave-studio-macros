' TEMHorn

Option Explicit
Sub Main()

	Const MaxPoints = 1000

	BeginHide
	StoreDoubleParameter "temhorn_which",   0
	StoreDoubleParameter "temhorn_points",  30
	StoreDoubleParameter "temhorn_L",       180

	StoreDoubleParameter "temhorn_Htype",   0
	StoreDoubleParameter "temhorn_Halpha",  0.5
	StoreDoubleParameter "temhorn_H0",      10
	StoreDoubleParameter "temhorn_HL",      75

	StoreDoubleParameter "temhorn_Btype",   0
	StoreDoubleParameter "temhorn_Balpha",  0.5
	StoreDoubleParameter "temhorn_B0",      10
	StoreDoubleParameter "temhorn_BL",      110

	StoreDoubleParameter "temhorn_Wtype",   0
	StoreDoubleParameter "temhorn_Walpha",  1
	StoreDoubleParameter "temhorn_W0",      50
	StoreDoubleParameter "temhorn_WL",      120*3.1416
	EndHide

	Dim which  As Integer
	Dim points As Double
	Dim L      As Double
	which  = CInt(RestoreDoubleParameter("temhorn_which"))
	points = CInt(RestoreDoubleParameter("temhorn_points"))
	L      = RestoreDoubleParameter("temhorn_L")

	Dim Htype, Halpha, H0, HL
	Htype  = CInt(RestoreDoubleParameter("temhorn_Htype"))
	Halpha = RestoreDoubleParameter("temhorn_Halpha")
	H0     = RestoreDoubleParameter("temhorn_H0")
	HL     = RestoreDoubleParameter("temhorn_HL")

	Dim Btype, Balpha, B0, BL
	Btype  = CInt(RestoreDoubleParameter("temhorn_Btype"))
	Balpha = RestoreDoubleParameter("temhorn_Balpha")
	B0     = RestoreDoubleParameter("temhorn_B0")
	BL     = RestoreDoubleParameter("temhorn_BL")

	Dim Wtype, Walpha, W0, WL
	Wtype  = CInt(RestoreDoubleParameter("temhorn_Wtype"))
	Walpha = RestoreDoubleParameter("temhorn_Walpha")
	W0     = RestoreDoubleParameter("temhorn_W0")
	WL     = RestoreDoubleParameter("temhorn_WL")

	Dim dx As Double
	Dim i As Integer
	Dim x As Double
	Dim logL0 As Double

	dx = L / points

	Dim reallyPoints As Integer
	reallyPoints = 0
	For x = 0 To L STEP dx
		reallyPoints = reallyPoints + 1
	Next

	Dim Hpoints(MaxPoints) As Double
	If which <> 1 Then
		If Htype = 0 Then
			i = 0
			logL0 = Log(HL/H0)
			For x = 0 To L STEP dx
				Hpoints(i) = H0 * Exp((x/L)^Halpha * logL0)
				i = i + 1
			Next
		Else
			i = 0
			For x = 0 To L STEP dx
				Hpoints(i) = (HL - H0)/L * x + H0
				i = i + 1
			Next
		End If
	End If

	Dim Bpoints(MaxPoints) As Double
	If which <> 0 Then
		If Btype = 0 Then
			i = 0
			logL0 = Log(BL/B0)
			For x = 0 To L STEP dx
				Bpoints(i) = B0 * Exp((x/L)^Balpha * logL0)
				i = i + 1
			Next
		Else
			i = 0
			For x = 0 To L STEP dx
				Bpoints(i) = (BL - B0)/L * x + B0
				i = i + 1
			Next
		End If
	End If

	Dim Wpoints(MaxPoints) As Double
	If which <> 2 Then
		If Wtype = 0 Then
			i = 0
			logL0 = Log(WL/W0)
			For x = 0 To L STEP dx
				Wpoints(i) = W0 * Exp((x/L)^Walpha * logL0)
				i = i + 1
			Next
		Else
			i = 0
			For x = 0 To L STEP dx
				Wpoints(i) = (WL - W0)/L * x + W0
				i = i + 1
			Next
		End If
	End If

	Dim sigma0 As Double
	sigma0 = 120*3.1416

	Select Case which
	Case 1
		i = 0
		For x = 0 To L STEP dx
			Hpoints(i) = Bpoints(i) * Wpoints(i) / sigma0
			i = i + 1
		Next
	Case 0
		i = 0
		For x = 0 To L STEP dx
			Bpoints(i) = Hpoints(i) * sigma0 / Wpoints(i)
			i = i + 1
		Next
	Case 2
		i = 0
		For x = 0 To L STEP dx
			Wpoints(i) = Hpoints(i) * sigma0 / Bpoints(i)
			i = i + 1
		Next
	Case Else
		' TODO
	End Select

	Dim x1 As Double
	Dim x2 As Double
	For i = 0 To reallyPoints-2
		x1 = i*dx
		x2 = x1 + dx
		With Polygon3D
			.Reset
			.Name  "temhorn_leaf"
			.Curve "temhorn_curve" & i
			.Point x1, -Bpoints(i)/2,   Hpoints(i)/2
			.Point x1,  Bpoints(i)/2,   Hpoints(i)/2
			.Point x2,  Bpoints(i+1)/2, Hpoints(i+1)/2
			.Point x2, -Bpoints(i+1)/2, Hpoints(i+1)/2
			.Point x1, -Bpoints(i)/2,   Hpoints(i)/2
			.Create
		End With
	Next

	For i = 0 To reallyPoints-2
		With CoverCurve
			.Reset
			.Name "leaf" & i
			.Component "component_antenna"
			.Material  "PEC"
			.Curve     "temhorn_curve" & i & ":temhorn_leaf"
			.Create
		End With
	Next

	For i = 0 To reallyPoints-3
		With Solid
			.Version 9
			.Add "component_antenna:leaf0", "component_antenna:leaf" & i+1
			.Version 1
		End With
	Next

	For i = 0 To reallyPoints-2
		Curve.DeleteCurve "temhorn_curve" & i
	Next

	With Transform
		.Reset
		.Name              "component_antenna"
		.Origin            "Free"
		.Center            "0", "0", "0"
		.PlaneNormal       "0", "0", "1"
		.MultipleObjects   "True"
		.GroupObjects      "False"
		.Repetitions       "1"
		.MultipleSelection "False"
		.Destination       ""
		.Material          ""
		.Transform         "Shape", "Mirror"
	End With

	With Solid
		.Version 9
		.Add "component_antenna:leaf0", "component_antenna:leaf0_1"
		.Version 1
	End With
End Sub

