# DIRECTIONS FOR USE

## Running the Listener

1. Open Listener.py.
2. Locate the occurrences of `ipv6` in the code (there should be around 3 instances).
3. Replace `ipv6` with the IPv6 address of the computer running the listener.

## Configuring User Locations

1. Find the `def setup_users` section in the code.
2. Set the home location using the format ""*location*"". For example, `"home/user/desktop"`.

## Sending Files

To send a file:

1. Place the file you want to send in the directory you specified in the user's location.
   
#

## Configuring The Address For backdoor To Connect To

1. Open the solution in Visual Studios and in Backdoor.cs look for the variable `ServerIPv6Address` and set it to the computer's address that is running the listener.
2. Build the application and it's ready to go.
