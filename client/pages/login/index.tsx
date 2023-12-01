import Input from "@/components/Input";
import {extractCookieAuthenticationData} from "@/src/services/token";
import {getUser} from "@/src/services/users";
import React, {FormEvent, MouseEventHandler, useCallback, useEffect, useState} from "react";
import Button from "@/components/Button";
import RightArrowIcon from "@/components/icons/RightArrowIcon";
import GoogleButton from "@/components/buttons/GoogleButton";
import MicrosoftButton from "@/components/buttons/MicrosoftButton";
import PasskeyButton from "@/components/buttons/PasskeyButton";
import {useRouter} from "next/router";
import Title from "@/components/general/Title";
import {passwordLoginUser, passwordRegisterUser} from "@/src/services/auth/passwords";
import {
	buildPasskeyAuthenticationConfiguration,
	confirmPasskeyAuthentication,
	createPasskeyAssertion
} from "@/src/services/auth/passkeys";
import {UserUnauthorizedResponse} from "@/src/models/user";

type AccountMenu = "login" | "register";

type AccountButtonProps = {
	onClick: MouseEventHandler<HTMLButtonElement>,
	title: string,
	selected: boolean
}

function AccountButton({onClick, title, selected}: AccountButtonProps) {
	const borderStyle = useCallback(() => selected ? "h-1 bg-neutral-800" : "h-0.5 bg-neutral-300", [selected])
	const textStyle = useCallback(() => selected ? "text-neutral-800" : "text-neutral-500", [selected])

	return (
		<button onClick={onClick} className={`flex flex-col flex-grow items-center justify-center`}>
			<h2 className={`p-2 font-medium text-md ${textStyle()}`}>
				{title}
			</h2>
			<div className={`flex w-full rounded-full ${borderStyle()}`}/>
		</button>
	)
}

export default function Login() {
	const [passKeyAvailable, setPassKeyAvailable] = useState<boolean>(false);
	const [selectedMenu, setSelectedMenu] = useState<AccountMenu>("login")

	const router = useRouter();

	async function passwordRegister(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();

		try {
			await passwordRegisterUser(
				event.currentTarget.username.value,
				event.currentTarget.email.value,
				event.currentTarget.password.value
			)

			router.reload();
		} catch (e) {
			console.log(e)
		}
	}

	async function passwordLogin(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();

		try {
			await passwordLoginUser(
				event.currentTarget.email.value,
				event.currentTarget.password.value
			);

			router.reload();
		} catch (e) {
			console.log(e)
		}
	}

	async function passkeyLogin() {
		let state = Math.random().toString(36).slice(2, 7);

		let configuration = await buildPasskeyAuthenticationConfiguration(state);
		let assertion = await createPasskeyAssertion(configuration);

		if (assertion === null)
			throw 'The given credential from passkey retrieval is invalid.'

		await confirmPasskeyAuthentication(state, assertion);

		router.reload();
	}


	async function googleLogin() {
		await router.push("http://localhost:5144/api/users/login/oidc/google")
	}

	async function microsoftLogin() {
		await router.push("http://localhost:5144/api/users/login/oidc/microsoft")
	}

	useEffect(() => {
		async function isPasskeyAvailable() {
			setPassKeyAvailable(
				window.PublicKeyCredential &&
				(await window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable()) &&
				(await window.PublicKeyCredential.isConditionalMediationAvailable())
			)
		}

		(async () => isPasskeyAvailable())()
	}, [])

	return (
		<>
			<div className={"flex flex-col p-8 w-[50%] bg-white rounded-xl shadow-lg"}>
				<div className={"flex flex-col gap-4"}>
					<Title>
						Account
					</Title>

					<div className={"flex flex-row columns-2"}>
						<AccountButton
							onClick={() => setSelectedMenu('login')}
							title={"Login"}
							selected={selectedMenu === 'login'}
						/>
						<AccountButton
							onClick={() => setSelectedMenu('register')}
							title={"Register"}
							selected={selectedMenu === 'register'}
						/>
					</div>

					{selectedMenu === 'login' ? (
						<form onSubmit={passwordLogin} className={"flex flex-col gap-4"}>
							<Input
								type={"email"}
								name={"email"}
								title={"Email"}
								placeholder={"email@account.com"}
								focus
								required
							/>

							<Input
								type={"password"}
								name={"password"}
								title={"Password"}
								placeholder={"secure"}
								required
							/>

							<Button
								type={"submit"}
								style={"primary"}
								icon={<RightArrowIcon className={"text-white w-4 h-4"}/>}
								title={"Continue"}
							/>
						</form>
					) : (
						<form onSubmit={passwordRegister} className={"flex flex-col gap-4"}>
							<Input
								name={"username"}
								title={"Username"}
								placeholder={"Alice"}
								focus
								required
							/>

							<Input
								type={"email"}
								name={"email"}
								title={"Email"}
								placeholder={"email@account.com"}
								required
							/>

							<Input
								type={"password"}
								name={"password"}
								title={"Password"}
								placeholder={"secure"}
								required
							/>

							<Button
								type={"submit"}
								style={"primary"}
								icon={<RightArrowIcon className={"text-white w-4 h-4"}/>}
								title={"Continue"}
							/>
						</form>
					)}
				</div>
			</div>
			<div className={"flex flex-col gap-4 p-8 w-[50%] bg-white rounded-xl shadow-lg"}>
				<GoogleButton onClick={() => googleLogin()}/>
				<MicrosoftButton onClick={() => microsoftLogin()}/>
				{passKeyAvailable && (
					<PasskeyButton onClick={() => passkeyLogin()}/>
				)}
			</div>
		</>
	)
}

export async function getServerSideProps({req}: any) {
	const [token, uuid] = extractCookieAuthenticationData(req.headers.cookie);

	try {
		const isUserUnauthorizedResponse = (u: any): u is UserUnauthorizedResponse => u.url !== undefined;

		const user = await getUser(uuid, token);
		const isUnauthorizedResponse = isUserUnauthorizedResponse(user);

		if (isUnauthorizedResponse)
			return {
				redirect: {
					permanent: false,
					destination: "/mfa",
				}
			}

		return {
			redirect: {
				permanent: false,
				destination: "/",
			}
		}

	} catch (e) {
		return {props: {}};
	}
}