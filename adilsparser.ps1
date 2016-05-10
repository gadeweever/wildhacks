function Read-HtmlPage {
    param ([Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)][String] $Uri)
    # Invoke-WebRequest and Invoke-RestMethod can't work properly with UTF-8 Response so we need to do things this way.
    [Net.HttpWebRequest]$WebRequest = [Net.WebRequest]::Create($Uri)
    [Net.HttpWebResponse]$WebResponse = $WebRequest.GetResponse()
    $Reader = New-Object IO.StreamReader($WebResponse.GetResponseStream())
    $Response = $Reader.ReadToEnd()
    $Reader.Close()
    # Create the document class
    [mshtml.HTMLDocumentClass] $Doc = New-Object -com "HTMLFILE"
    $Doc.IHTMLDocument2_write($Response)
    # Returns a HTMLDocumentClass instance just like Invoke-WebRequest ParsedHtml
    $Doc
}

function Get-Article{ 
    #$Difficulty = "beginner"
    #$ArticleNumber = 1
    param($Difficulty, [int]$ArticleNumber)
    if ($Difficulty -eq "beginner"){
        $url = "$Difficulty/$Difficulty-readings/$Difficulty-reading$ArticleNumber.html"
    }
    elseif($Difficulty -eq "intermediate"){
        $url = "$Difficulty/readings/reading$ArticleNumber.html"
    }
    elseif($Difficulty -eq "advanced"){
        $url = "NULL"
    }

    $Website = Read-HtmlPage("http://www.learnpracticalspanishonline.com/$url")
    $WebsiteString = $Website.body.outerHTML
    $TitleStart = $WebsiteString.IndexOf("<H2>")
    $TitleEnd = $WebsiteString.IndexOf("</H2>")
    $Title = $WebsiteString.Substring($TitleStart+4,$TitleEnd-($TitleStart+4))
    $ContentStart = $WebsiteString.IndexOf('<DIV id=mainContent>')
    $WebsiteString = $WebsiteString.Substring($ContentStart)
    $Content = ""
    $i = 1
    
    do{   
        $ContentStart = $WebsiteString.IndexOf("<P>")
        $WebsiteString = $WebsiteString.Substring($ContentStart+3)
        if($WebsiteString.Substring(0,1) -eq "<"){
            $ContentStart = $WebsiteString.IndexOf("<P>")
            $WebsiteString = $WebsiteString.Substring($ContentStart+3)
        }
        $ContentEnd = $WebsiteString.IndexOf("</P>")
        if ($i % 2 -ne 0){  
            $Content = $Content + "`n" + $WebsiteString.Substring(0,$ContentEnd)
            $WebsiteString = $WebsiteString.Substring(1)         
        }
        $i = $i + 1       
    }
    while($Content.Length -lt 1000)
    
    "Title: " + $Title + "`n"
    "Content: " + $Content + "`n`n`n"
}

#DONT USE ARTICLE 1 Get-Article "beginner" 1
#Get-Article "beginner" 2
#Get-Article "beginner" 3
#Get-Article "beginner" 4
#Get-Article "beginner" 5
#Get-Article "intermediate" 1
#Get-Article "intermediate" 2
#Get-Article "intermediate" 3
#Get-Article "intermediate" 4




