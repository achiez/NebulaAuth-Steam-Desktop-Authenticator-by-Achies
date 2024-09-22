# Steam Guard Linking Issue Solution

Some accounts may occasionally face issues when trying to link Steam Guard. It may be impossible to link such accounts without an SMS, and sometimes even with the correct SMS code, it won't be accepted. Common errors include (error names may vary in other SDA clients):

- BadConfirmationCode
- InvalidState
- InvalidStateWithStatus2
- Code did not match
- And others

This error most often occurs when entering the code from an email, but sometimes it can appear when trying to add a phone number or inputting an SMS code.

## Solution (it’s important to follow the steps precisely)

1. Once the error is encountered, you need to **restart the Steam Guard linking process** from the beginning.
2. Enter your login and password, then you’ll be asked to input a phone number. **Do not ignore** this request and enter a real phone number.
3. From here, two things can happen: either you get another error, or you’re asked to enter an SMS code.
   - If you get another error, proceed to step 4.
   - If you receive an SMS code, **do not enter it**, just close the linking window (see step 3.1).

   **3.1. Important!** Do not enter the SMS code you received. Close the linking window.  
   **3.2. Wait 20 minutes** for Steam to "forget" about the phone number in the linking process.
   
4. At this point, the bug should be cleared from your account.
5. Now, you can retry linking the account as usual. The problem should be resolved, and you can link the account with or without SMS.


If this doesn’t help, try again. However, be aware that after 3-5 failed attempts, Steam may block the linking process for anywhere from one day to a week. To avoid this, it’s best to follow this guide as soon as the issue is detected.
