# AuctionEdge Developer Access

## Introduction

AuctionEdge provides developers access to auction data through a GraphQL based API. Developers are only allowed access to data they are authorized for. This document will describe the basics for this access. The ./samples directory contains example code for this in various languages.

## Authentication

AuctionEdge will have provided you with the following authentication information to allow you access to the GraphQL API. You will receive more than one as Auctionedge provides an environment for you to test in that is not our production environment. The values are entirely environment dependent so plan accordingly when writing your code.

| Name      | Description                                      |
| --------- | ------------------------------------------------ |
| username  | Your user name needed for authentication         |
| password  | Your password                                    |
| client_id | ClientId value for authentication                |
| api_host  | The name of the machine hosting the api endpoint |

These values will be represented in the following code snippets inside double curly braces like this `{{ variable_name }}` The code snippets are not usable as-is and this substitution must be performed manually.

### How to get your authorization token

To acquire an authorization (authz) token the user must make a request to authenticate with AWS Cognito.

#### HTTP

```http
POST / HTTP/1.1
Host: cognito-idp.us-west-2.amazonaws.com
X-Amz-Target: AWSCognitoIdentityProviderService.InitiateAuth
Content-Type: application/x-amz-json-1.1

{
    "AuthParameters": {
        "USERNAME": "{{username}}",
        "PASSWORD": "{{password}}"
    },
    "AuthFlow": "USER_PASSWORD_AUTH",
    "ClientId": "{{client_id}}"
}
```

#### curl

```sh
curl --location --request POST 'https://cognito-idp.us-west-2.amazonaws.com' \
--header 'X-Amz-Target: AWSCognitoIdentityProviderService.InitiateAuth' \
--header 'Content-Type: application/x-amz-json-1.1' \
--data-raw '{
    "AuthParameters": {
        "USERNAME": "{{username}}",
        "PASSWORD": "{{password}}"
    },
    "AuthFlow": "USER_PASSWORD_AUTH",
    "ClientId": "{{client_id}}"
}'
```

### Response

The response is in JSON and the user will need to extract the `AccessToken` value from the response and include it in the Authorization header of the API request.
The `ExpiresIn` property of the response describes how long the token is valid for in seconds. It is currently set to 3600 seconds, or 1 hour (subject to change).

#### Example JSON

```json
{
  "AuthenticationResult": {
    "AccessToken": "eyJraWQiOiI1RjYyeGZvalIxTWNOTjlhdlwvY2FES2N4NUVEUnFnSkpJZXU4MEtrSlJEaz0iLCJhbGciOiJSUzI1NiJ9...",
    "ExpiresIn": 3600,
    "IdToken": "eyJraWQiOiJyaTd4SHBYeXoyYm15cTl4aTBJbFBNWW54Ullid3ZhdURFdVBlTzdheldZPSIsImFsZyI6IlJTMjU2In0...",
    "RefreshToken": "eyJjdHkiOiJKV1QiLCJlbmMiOiJBMjU2R0NNIiwiYWxnIjoiUlNBLU9BRVAifQ...",
    "TokenType": "Bearer"
  },
  "ChallengeParameters": {}
}
```

## How to query the AuctionEdge GraphQL

You can make a graphql request to the Auction Edge API once you’ve been issued an authz token. Examples here are at the basic HTTP level. Your preferred programming language may give you access to libraries or frameworks that allow you to make these calls also. The example provided here does not reflect the current GraphQL schema and are used to just show the process.

The current graphql schema is partially captured [here](./auction-schema.graphql)

[Here](https://graphql.org/learn/queries/) is a discussion on graphql from a users perspective.

Note: "auction-code" in the following examples will need to be changed to a valid auction code. All date/timestamp values must be in the ISO 8601 format (e.g. 2025-01-03T07:59:59.999Z).

#### HTTP

```HTTP
POST /graphql HTTP/1.1
Host: {{api_host}}
x-amz-user-agent: aws-amplify/2.0.1
content-type: [{"key":"content-type","value":"application/json","enabled":true}]
accept-language: en-US,en;q=0.9
Authorization: {{AccessToken or IdToken}}

{ "query": "query test { auction(id: \"auction-code\") { assets { purchased(pageRequest: { pageSize: 10, pageNumber: 1 }) { items { vin year make model exteriorColor } } } } }" }
```

#### curl

```sh
curl --location --request POST 'https://{{api_host}}/graphql' \
--header 'x-amz-user-agent: aws-amplify/2.0.1' \
--header 'content-type: [{"key":"content-type","value":"application/json","enabled":true}]' \
--header 'accept-language: en-US,en;q=0.9' \
--header 'Authorization: {{AccessToken or IdToken}}' \
--data-raw '{ "query": "query test { auction(id: \"auction-code\") { assets { purchased(pageRequest: { pageSize: 10, pageNumber: 1 }) { items { vin year make model exteriorColor } } } } }" }'
```

#### Response

This is an example JSON response from the above non-working examples. Note that the data is returned in the `data` field in a format that matches the schema and requested fields from the query.

```json
{
  "data": {
    "auction": {
      "assets": {
        "purchased": {
          "items": [
            {
              "vin": "1GKS2HKJ3JR259932",
              "year": "2018",
              "make": "GMC",
              "model": "Yukon XL",
              "exteriorColor": "White"
            }
          ]
        }
      }
    }
  }
}
```
