using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TwittorApp.Dtos;
using TwittorApp.Kafka;
using TwittorApp.Models;

namespace TwittorApp.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MEMBER"})]
        public async Task<TransactionStatus> AddTwitAsync(
           InputTwittor input,
           [Service] Kasus2DbContext context,
           [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var twittor = new Twittor
            {
                UserId = input.UserId,
                Post = input.Post,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };
            var key = "Add-Twit-" + DateTime.Now.ToString();
            var val = JObject.FromObject(twittor).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "twittor", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<TransactionStatus> AddCommentAsync(
           InputComment input,
           [Service] Kasus2DbContext context,
           [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var comment = new Comment
            {
                TwittorId = input.TwittorId,
                UserId = input.UserId,
                Comment1 = input.Comment1,
                Created = DateTime.Now,
                Updated = DateTime.Now 
            };

            var key = "Add-Comment-" + DateTime.Now.ToString();
            var val = JObject.FromObject(comment).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "comment", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<TransactionStatus> DeleteTwitAsync(
            int twittorId,
            InputTwittor input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var twittor = context.Twittors.Where(o => o.Id == twittorId).FirstOrDefault();
            if (twittor != null)
            {
                context.Twittors.Remove(twittor);
                await context.SaveChangesAsync();
            }
            var key = "Delete-Twit-" + DateTime.Now.ToString();
            var val = JObject.FromObject(twittor).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "twittor", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        public async Task<InputCommentPayload> DeleteCommentAsync(
            int id,
            InputComment input,
            [Service] Kasus2DbContext context)
        {
            var comment = context.Comments.Where(o => o.Id == id).FirstOrDefault();
            if (comment != null)
            {
                context.Comments.Remove(comment);
                await context.SaveChangesAsync();
            }

            return new InputCommentPayload(comment);
        }

        [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<InputUserPayload> UpdateUserAsync(
            int id,
            InputUser input,
            [Service] Kasus2DbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                user.FullName = input.FullName;
                user.Email = input.Email;
                user.Username = input.Username;
                user.Updated = DateTime.Now;

                await context.SaveChangesAsync();
            }

            return new InputUserPayload(user);
        }

        [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<InputUserPayload> UpdateUserPasswordAsync(
            int id,
            InputUser input,
            [Service] Kasus2DbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);
                user.Updated = DateTime.Now;

                await context.SaveChangesAsync();
            }

            return new InputUserPayload(user);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<InputUserPayload> LockUserAsync(
            int id,
            InputUser input,
            [Service] Kasus2DbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                user.IsLocked = input.IsLocked;
                user.Updated = DateTime.Now;

                await context.SaveChangesAsync();
            }

            return new InputUserPayload(user);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<InputUserRolePayload> UpdateUserRoleAsync(
            int id,
            InputUserRole input,
            [Service] Kasus2DbContext context)
        {
            var userRole = context.UserRoles.Where(o => o.UserId == id).FirstOrDefault();
            if (userRole != null)
            {
                userRole.RoleId = input.RoleId;

                await context.SaveChangesAsync();
            }

            return new InputUserRolePayload(userRole);
        }

        public async Task<UserData> RegisterUserAsync(
            RegisterUser input,
            [Service] Kasus2DbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
                Created = DateTime.Now,
                Updated = DateTime.Now,
                //IsLocked = true
            };
  
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        /* public async Task<ConfirmUserPayload> ConfirmUserAndAddRoleAsync(
             int id,
             [Service] Kasus2DbContext context)
         {
             var userConfirm = context.Users.Where(u => u.Id == id).FirstOrDefault();
             if (userConfirm != null)
             {
                 userConfirm.IsLocked = false;
                 await context.SaveChangesAsync();
             }
             var userRole = context.UserRoles.Where(o => o.UserId == id).FirstOrDefault();
             if (userRole != null)
             {
                 userRole.RoleId = 1;
                 await context.SaveChangesAsync();
             }
             await context.SaveChangesAsync();

             return new ConfirmUserPayload(userConfirm);
         }
        */
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<InputUserRolePayload> AddUserRoleAsync(
           InputUserRole input,
           
           [Service] Kasus2DbContext context)
        {
            var userRole = new UserRole
            {
                UserId = (int)input.UserId,
                RoleId = input.RoleId
            };

            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            return new InputUserRolePayload(userRole);
        }

        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] Kasus2DbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims,
                    signingCredentials: credentials
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }



    }
}
