'user params
Dim buses = New String() {"master", "busA"} 'master/busA/BusB/etc
Dim inputs = New String() {"meterM.gtzip", "meterA.gtzip"} 'Names of title for each bus
Dim updateDelay = 10

'prefixes, in case if you want to customize title
'notice, that full name of textfield will be "N_chl_grn_val.Text"
'where N is audiochannel number
Dim greenTextField As String = "_chl_grn_val.Text"
Dim yellowTextField As String = "_chl_ylw_val.Text"
Dim redTextField As String = "_chl_red_val.Text"
Dim dbfsTextField As String = "_dbfs_val.Text"
Dim busNameTextField As String = "bus_name_val.Text"

Dim meterYellowThreshold = 84 'volume threshold in percentage
Dim meterRedThreshold = 98
'user params end

If buses.Length <> inputs.Length
  Console.WriteLine("Buses amount must be equal inputs amount")
  return
End If

Dim doc As New XmlDocument()
Dim busNode As XmlNode

Dim regionalDelimeter As Char = "."
If not Double.TryParse("0.999", new Double)
   regionalDelimeter = ","
End If

Dim dbfs As Double
Dim channelsAmount = 2
Dim channelTextVals (3) As String '3 for green/yellow/red bars
Dim channelAmplitudes (channelsAmount) As Double
Dim channelVolumes (channelsAmount) As Integer
Dim channelDbfs (channelsAmount) As Double

While True
    doc.LoadXml(API.Xml)
    For busIndex As Integer = 0 To buses.Length - 1
      Dim bus As String = buses(busIndex)
      Dim input As String = inputs(busIndex)
      busNode = doc.DocumentElement.SelectSingleNode("/vmix/audio/" + bus)
      
      channelAmplitudes(0) = Convert.ToDouble(busNode.Attributes.ItemOf("meterF1").Value.Replace(".", regionalDelimeter))
      channelAmplitudes(1) = Convert.ToDouble(busNode.Attributes.ItemOf("meterF2").Value.Replace(".", regionalDelimeter))

      Dim busName As String = bus
      If busNode.Attributes.ItemOf("name") IsNot Nothing
       busName = busNode.Attributes.ItemOf("name").Value
      End If
      API.Function("SetText", Input:=input, Value:=busName, SelectedName:=busNameTextField)

      For i As Integer = 0 To channelsAmount - 1
        channelVolumes(i) = Math.Round(Math.Pow(channelAmplitudes(i), 0.25) * 100, 0)
        channelDbfs(i) = 20 * Math.log(channelAmplitudes(i), 10)

        API.Function("SetText", Input:=input, Value:=If(channelDbfs(i) <= -100, "-Inf", channelDbfs(i).ToString("0.0")), SelectedName:=i.ToString + dbfsTextField)

        channelTextVals(0) = new String("0", Math.Min(channelVolumes(i), meterYellowThreshold))
        channelTextVals(1) = new String("0", Math.Min(channelVolumes(i), meterRedThreshold))
        channelTextVals(2) = new String("0", channelVolumes(i))

        API.Function("SetText", Input:=input, Value:=channelTextVals(0), SelectedName:=i.ToString() + greenTextField)
        API.Function("SetText", Input:=input, Value:=channelTextVals(1), SelectedName:=i.ToString() + yellowTextField)
        API.Function("SetText", Input:=input, Value:=channelTextVals(2), SelectedName:=i.ToString() + redTextField)
      Next
    Next

    Sleep(updateDelay)
End While