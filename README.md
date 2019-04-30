# telesign_voice_demo_in_csharp
Set up a simple C# web server and see the play and collect digits feature of the TeleSign Voice API. 

# Notes
The simple C# web server is great for getting started with development right away, with a minimum of effort. If you plan to put the TeleSign Voice API into production, you should choose something more robust. 

# Resources
You can learn more about the TeleSign Voice API here: https://enterprise.telesign.com/api-reference/apis/voice

# Requirements 
To use this demo: 

* You can, if you wish, mimic trying out the TeleSign Voice API by faking a request to the server. You can use example payloads from the [Voice API Reference documentation](https://enterprise.telesign.com/api-reference/apis/voice/api-reference) and send them to your server using something like [Postman](https://www.getpostman.com/) or a cURL request or similar. 
* You can sign up for an account here - https://info.telesign.com/Voice.html 

If you already have your account, there's a few things you'll need: 

* Customer ID and API key - available from TeleSign through your Customer Success Manager. 
* TeleSign phone number - you must purchase a TeleSign phone number to use as a callerID. You can do this using the Phone Numbers API. Ask your Customer Success Manager to grant access to the Phone Numbers API, and then check out the [Get Started with the Phone Numbers API](https://enterprise.telesign.com/api-reference/apis/phone-numbers-api/get-started) page. 
* Customer event URL on your server for TeleSign to post event information to. For more details, refer to the [Set up the Customer Event URL](https://enterprise.telesign.com/api-reference/apis/voice/api-reference#set-up-the-customer-event-url) section on the Voice API reference page. 

# Get Started
If you are all set with the requirements, you are ready to get started: 

1. Download this repository to your computer. The project was built using Visual Studio Code. 
2. Open the **CSharpServer** folder. 
3. In the Program.cs file, scroll to the **internal class Program** section. The demo code for TeleSign's Voice API is added here in the **SendResponse** method.
4. You will need to set a few phone numbers. In the **if (postBodyEvent == SPEAK_COMPLETED)** section where it says **Your CallerID number**. Keep the quotes and put the appropriate number between them for **callerIdNum**, **customerServiceNum**, and **financeDeptNum**. 
5. The code shows how you receive a request from TeleSign's API and process it. Included are the Dial, Speak, and Speak and Collect Digits commands. Using these, the sample takes an incoming request, plays a message for the caller using Text-to-Speech, collects a choice from the caller and then connects them to either the Customer Service or Finance Department.
6. To see the calls demonstrated, you will need to have your customer event URL set up with TeleSign, and a TeleSign purchased callerID assigned to your system. 

# Thanks 
This project uses a simple web server that's described here - https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server. The TeleSign code is added to the SendResponse method of this sample server. 
